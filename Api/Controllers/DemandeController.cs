using Data.Context;
using Domain.Dtos;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DemandeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DemandeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── GET ALL ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
                var demandes = await _context.Demandes
                .OrderByDescending(d => d.DateDemande)
                .ToListAsync();
            return Ok(demandes);
        }

        // ── GET BY ID ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();
            return Ok(demande);
        }

        // ── POST — Créer demande ──────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateDemandeDto dto)
        {
            if (dto == null) return BadRequest();

            var count = await _context.Demandes.CountAsync();
            var demande = new Demande
            {
                Description          = dto.Description,
                FilialeId            = dto.FilialeId,
                TypeProduit          = dto.TypeProduit,
                NombreEchantillons   = dto.NombreEchantillons,
                Priorite             = dto.Priorite,
                UtilisateurId        = dto.UtilisateurId,
                SlaHeures            = dto.SlaHeures,
                DateDemande          = DateTime.Now,
                Statut               = "En attente",
                Reference            = $"DEM-{DateTime.Now:yyyyMMdd}{(count + 1):D6}",
                NumeroDeLot          = dto.NumeroDeLot,
                LotBloque            = false,
                ContreAnalyseRequise = false,
                StatutLot            = null,
                CommentaireLabo      = string.Empty,
                CommentaireRejet     = string.Empty,
                ValidePar            = string.Empty,
                NumeroCertificat     = string.Empty,
                MotifBlocage         = string.Empty
            };

            _context.Demandes.Add(demande);
            await _context.SaveChangesAsync();

            await AjouterAudit(demande.Id, demande.UtilisateurId, "CREATION",
                $"Demande créée — Produit: {demande.TypeProduit}, Filiale: {demande.FilialeId}");

            return Ok(new { message = "Demande créée avec succès", demande });
        }

        // ── PUT — Modifier demande ────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Demande demande)
        {
            if (demande == null) return BadRequest();
            demande.Id = id;
            _context.Entry(demande).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Demande modifiée avec succès" });
        }

        // ── DELETE ────────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();
            _context.Demandes.Remove(demande);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Demande supprimée" });
        }

        // ── RÉCEPTION ─────────────────────────────────────────
        [HttpPost("{id}/reception")]
        public async Task<IActionResult> Reception(int id, [FromBody] ReceptionDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.Statut = "Reçu";
            demande.TechnicienId = dto.TechnicienId;
            demande.DateRenduEstimee = dto.DateRenduEstimee;
            demande.CommentaireLabo = dto.Commentaire;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.TechnicienId ?? 0, "RECEPTION",
                $"Demande reçue — Date rendu estimée: {dto.DateRenduEstimee:dd/MM/yyyy}");

            return Ok(new { message = "Demande reçue", demande });
        }

        // ── DÉMARRER ──────────────────────────────────────────
        [HttpPost("{id}/demarrer")]
        public async Task<IActionResult> Demarrer(int id)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (demande.Statut != "Reçu")
                return BadRequest(new { message = $"Impossible de démarrer : statut actuel '{demande.Statut}' (attendu : 'Reçu')" });

            demande.Statut = "En cours";
            await _context.SaveChangesAsync();

            await AjouterAudit(id, demande.TechnicienId ?? 0, "DEMARRAGE",
                "Analyse démarrée par le technicien");

            return Ok(new { message = "Analyse démarrée", demande });
        }

        // ── TERMINER ──────────────────────────────────────────
        [HttpPost("{id}/terminer")]
        public async Task<IActionResult> Terminer(int id, [FromBody] TerminerDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (demande.Statut != "En cours" && demande.Statut != "Contre-analyse")
                return BadRequest(new { message = $"Impossible de soumettre : statut actuel '{demande.Statut}' (attendu : 'En cours' ou 'Contre-analyse')" });

            demande.Statut = "En validation";
            demande.CommentaireLabo = dto.Commentaire;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, demande.TechnicienId ?? 0, "SOUMISSION",
                "Analyse soumise pour validation");
            var responsables = await _context.Utilisateurs
    .Where(u => u.Role == "ResponsableLabo").ToListAsync();
            foreach (var r in responsables)
            {
                await AjouterNotification(r.Id,
                    "Analyse soumise pour validation",
                    $"La demande {demande.Reference} ({demande.TypeProduit}) est prête pour validation.",
                    "info", id);
            }

            return Ok(new { message = "Analyse soumise pour validation", demande });
        }

        // ── APPROUVER (approbation directe Admin uniquement) ──
        [HttpPost("{id}/approuver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approuver(int id, [FromBody] ApprobationDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (demande.Statut != "En validation" && demande.Statut != "Validé Responsable")
                return BadRequest(new { message = $"Impossible d'approuver : statut actuel '{demande.Statut}'" });

            // Générer le numéro de certificat
            var countCert = await _context.Demandes
                .CountAsync(d => d.NumeroCertificat != null && d.NumeroCertificat != "");
            demande.NumeroCertificat = $"CERT-{DateTime.Now.Year}-{(countCert + 1):D5}";
            demande.Statut = "Approuvé";
            demande.DateValidation = DateTime.Now;
            demande.ValidePar = dto.NomAdmin;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.AdminId, "APPROBATION",
                $"Analyse approuvée — Certificat : {demande.NumeroCertificat}",
                dto.NomAdmin);
            await AjouterNotification(
    demande.UtilisateurId,
    "Analyse approuvée",
    $"La demande {demande.Reference} a été approuvée. Certificat : {demande.NumeroCertificat}",
    "success", id
);

            return Ok(new
            {
                message = "Analyse approuvée",
                demande,
                certificat = demande.NumeroCertificat
            });
        }

        // ── REJETER ───────────────────────────────────────────
        [HttpPost("{id}/rejeter")]
        public async Task<IActionResult> Rejeter(int id, [FromBody] RejetDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.Statut = "Rejeté";
            demande.CommentaireRejet = dto.Motif;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.AdminId, "REJET",
                $"Analyse rejetée — Motif : {dto.Motif}", dto.NomAdmin);
            await AjouterNotification(
    demande.UtilisateurId,
    "Analyse rejetée",
    $"La demande {demande.Reference} a été rejetée. Motif : {dto.Motif}",
    "danger", id
);
            if (demande.TechnicienId.HasValue)
            {
                await AjouterNotification(
                    demande.TechnicienId.Value,
                    "Analyse rejetée",
                    $"Votre analyse sur {demande.Reference} a été rejetée. Motif : {dto.Motif}",
                    "warning", id
                );
            }

            return Ok(new { message = "Analyse rejetée", demande });
        }

        // ── CLÔTURER ──────────────────────────────────────────
        [HttpPost("{id}/cloturer")]
        public async Task<IActionResult> Cloturer(int id)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.Statut = "Clôturé";
            await _context.SaveChangesAsync();

            int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var currentUserId);
            await AjouterAudit(id, currentUserId, "CLOTURE", "Demande clôturée");

            return Ok(new { message = "Demande clôturée", demande });
        }

        // ── ASSIGNER TECHNICIEN ───────────────────────────────
        [HttpPost("{id}/assigner")]
        public async Task<IActionResult> AssignerTechnicien(int id,
            [FromBody] AssignationDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.TechnicienId = dto.TechnicienId;
            demande.TechnicienAssigneParId = dto.AssigneParId;
            demande.DateAssignation = DateTime.Now;
            demande.Statut = "Reçu";

            // SLA — date de rendu estimée automatique
            if (demande.SlaHeures > 0)
                demande.DateRenduEstimee = DateTime.Now.AddHours(demande.SlaHeures);

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.AssigneParId,
                "ASSIGNATION",
                $"Demande assignée au technicien ID {dto.TechnicienId} — SLA: {demande.SlaHeures}h",
                dto.NomAssignePar);


            await AjouterNotification(
    dto.TechnicienId,
    "Nouvelle analyse assignée",
    $"La demande {demande.Reference} ({demande.TypeProduit}) vous a été assignée. SLA : {demande.SlaHeures}h",
    "info", id
);

            return Ok(new { message = "Technicien assigné avec succès", demande });
        }

        // ── BLOQUER LOT ───────────────────────────────────────
        [HttpPost("{id}/bloquer")]
        public async Task<IActionResult> BloquerLot(int id, [FromBody] BlocageDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.LotBloque = true;
            demande.DateBlocage = DateTime.Now;
            demande.MotifBlocage = dto.Motif;
            demande.Statut = "Lot bloqué";

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.UserId,
                "BLOCAGE LOT",
                $"Lot bloqué — Motif : {dto.Motif}",
                dto.NomUtilisateur);

            await AjouterNotification(
    demande.UtilisateurId,
    "Lot bloqué",
    $"Le lot de la demande {demande.Reference} a été bloqué. Motif : {dto.Motif}",
    "danger", id
);
            return Ok(new { message = "Lot bloqué avec succès", demande });
        }

        // ── VALIDER PAR RESPONSABLE (niveau 1) ───────────────
        [HttpPost("{id}/valider-responsable")]
        public async Task<IActionResult> ValiderResponsable(int id,
            [FromBody] ValidationResponsableDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (demande.Statut != "En validation")
                return BadRequest(new { message = $"Validation impossible : statut actuel '{demande.Statut}' (attendu : 'En validation')" });

            demande.Statut = "Validé Responsable";
            demande.DateValidationResponsable = DateTime.Now;
            demande.ValideParResponsableId = dto.ResponsableId;
            demande.CommentaireLabo = dto.Commentaire ?? demande.CommentaireLabo;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.ResponsableId, "VALIDATION RESPONSABLE",
                $"Validé par le Responsable Labo — {dto.Commentaire ?? "Sans commentaire"}",
                dto.NomResponsable);

            // Notifier les directeurs qualité
            var directeurs = await _context.Utilisateurs
                .Where(u => u.Role == "DirecteurQualite").ToListAsync();
            foreach (var dir in directeurs)
            {
                await AjouterNotification(dir.Id,
                    "Analyse en attente d'approbation finale",
                    $"La demande {demande.Reference} ({demande.TypeProduit}) a été validée par le responsable et requiert votre approbation finale.",
                    "info", id);
            }

            return Ok(new { message = "Analyse validée par le responsable", demande });
        }

        // ── VALIDER PAR DIRECTEUR (niveau 2 — approbation finale) ──
        [HttpPost("{id}/valider-directeur")]
        public async Task<IActionResult> ValiderDirecteur(int id,
            [FromBody] ValidationDirecteurDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (demande.Statut != "Validé Responsable" && demande.Statut != "En validation")
                return BadRequest(new { message = $"Approbation finale impossible : statut actuel '{demande.Statut}'" });

            // Générer le certificat
            var countCert = await _context.Demandes
                .CountAsync(d => d.NumeroCertificat != null && d.NumeroCertificat != "");
            demande.NumeroCertificat = $"CERT-{DateTime.Now.Year}-{(countCert + 1):D5}";
            demande.Statut = "Approuvé";
            demande.DateValidation = DateTime.Now;
            demande.DateValidationDirecteur = DateTime.Now;
            demande.ValideParDirecteurId = dto.DirecteurId;
            demande.ValidePar = dto.NomDirecteur;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.DirecteurId, "APPROBATION DIRECTEUR",
                $"Approbation finale — Certificat : {demande.NumeroCertificat}",
                dto.NomDirecteur);

            await AjouterNotification(demande.UtilisateurId,
                "Analyse approuvée",
                $"La demande {demande.Reference} a été approuvée. Certificat : {demande.NumeroCertificat}",
                "success", id);

            if (demande.ValideParResponsableId.HasValue)
            {
                await AjouterNotification(demande.ValideParResponsableId.Value,
                    "Approbation finale émise",
                    $"La demande {demande.Reference} a été définitivement approuvée par la direction. Certificat : {demande.NumeroCertificat}",
                    "success", id);
            }

            return Ok(new { message = "Approbation finale émise", demande, certificat = demande.NumeroCertificat });
        }

        // ── REJETER (DirecteurQualite) ────────────────────────
        [HttpPost("{id}/rejeter-directeur")]
        public async Task<IActionResult> RejeterDirecteur(int id,
            [FromBody] RejetDirecteurDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.Statut = "Rejeté";
            demande.CommentaireRejet = dto.Motif;
            demande.CommentaireRejetDirecteur = dto.Motif;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.DirecteurId, "REJET DIRECTEUR",
                $"Rejeté par la direction — Motif : {dto.Motif}", dto.NomDirecteur);

            await AjouterNotification(demande.UtilisateurId,
                "Demande rejetée",
                $"La demande {demande.Reference} a été rejetée. Motif : {dto.Motif}",
                "danger", id);

            if (demande.ValideParResponsableId.HasValue)
            {
                await AjouterNotification(demande.ValideParResponsableId.Value,
                    "Demande rejetée par la direction",
                    $"La demande {demande.Reference} a été rejetée. Motif : {dto.Motif}",
                    "warning", id);
            }

            if (demande.TechnicienId.HasValue)
            {
                await AjouterNotification(demande.TechnicienId.Value,
                    "Demande rejetée par la direction",
                    $"La demande {demande.Reference} a été rejetée. Motif : {dto.Motif}",
                    "warning", id);
            }

            return Ok(new { message = "Demande rejetée par la direction", demande });
        }

        // ── HISTORIQUE (alias /historique → AuditLogs) ────────
        [HttpGet("{id}/historique")]
        public async Task<IActionResult> GetHistorique(int id)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.DemandeId == id)
                .OrderBy(a => a.DateAction)
                .ToListAsync();
            return Ok(logs);
        }

        // ── RÉOUVRIR ──────────────────────────────────────────
        [HttpPost("{id}/reouvrir")]
        public async Task<IActionResult> Reouvrir(int id, [FromBody] ReouvrirDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.Statut = "Reçu";
            demande.CommentaireReouvrir = dto.Commentaire;
            demande.DateReouverte = DateTime.Now;

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.UserId, "REOUVERTURE",
                $"Demande réouverte — Commentaire : {dto.Commentaire}", dto.NomUtilisateur);

            if (demande.TechnicienId.HasValue)
            {
                await AjouterNotification(demande.TechnicienId.Value,
                    "Demande réouverte",
                    $"La demande {demande.Reference} a été réouverte pour compléments d'analyse. Commentaire : {dto.Commentaire}",
                    "warning", id);
            }

            return Ok(new { message = "Demande réouverte avec succès", demande });
        }

        // ── RELANCER TECHNICIEN ───────────────────────────────
        [HttpPost("{id}/relancer")]
        public async Task<IActionResult> Relancer(int id, [FromBody] BaseActionDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            if (!demande.TechnicienId.HasValue)
                return BadRequest(new { message = "Aucun technicien assigné à cette demande" });

            await AjouterNotification(demande.TechnicienId.Value,
                "Relance — Demande en retard",
                $"La demande {demande.Reference} ({demande.TypeProduit}) dépasse le délai SLA de {demande.SlaHeures}h. Veuillez traiter en priorité.",
                "danger", id);

            await AjouterAudit(id, dto.UserId, "RELANCE",
                "Technicien relancé pour dépassement de délai SLA", dto.NomUtilisateur);

            return Ok(new { message = "Notification de relance envoyée au technicien" });
        }

        // ── CONTRE-ANALYSE ────────────────────────────────────
        [HttpPost("{id}/contre-analyse")]
        public async Task<IActionResult> ContreAnalyse(int id, [FromBody] BaseActionDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.ContreAnalyseRequise = true;
            demande.Statut = "Contre-analyse";

            await _context.SaveChangesAsync();

            await AjouterAudit(id, dto.UserId,
                "CONTRE-ANALYSE",
                "Contre-analyse demandée suite à anomalie critique",
                dto.NomUtilisateur);

            return Ok(new { message = "Contre-analyse demandée", demande });
        }

        // ── BLOQUER LOT (StatutLot) ───────────────────────────
        [HttpPost("{id}/bloquer-lot")]
        public async Task<IActionResult> BloquerLotStatut(int id, [FromBody] BloquerLotNouveauDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.StatutLot = "Bloqué";
            demande.MotifBlocageLot = dto.Motif;
            demande.DateBlocageLot  = DateTime.Now;
            await _context.SaveChangesAsync();

            // Notifier tous les DirecteurQualite
            var directeurs = await _context.Utilisateurs
                .Where(u => u.Role == "DirecteurQualite").ToListAsync();
            foreach (var d in directeurs)
                await AjouterNotification(d.Id,
                    "Lot bloqué",
                    $"Le lot de {demande.Reference} ({demande.TypeProduit}) a été bloqué. Motif : {dto.Motif}",
                    "danger", id);

            // Notifier la Production (demandeur)
            await AjouterNotification(demande.UtilisateurId,
                "Lot bloqué",
                $"Le lot de votre demande {demande.Reference} a été bloqué. Motif : {dto.Motif}",
                "danger", id);

            int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var auditorIdBlocage);
            await AjouterAudit(id, auditorIdBlocage, "BLOCAGE LOT",
                $"StatutLot → Bloqué — Motif : {dto.Motif}");

            return Ok(new { message = "Lot bloqué", demande });
        }

        // ── LIBÉRER LOT (StatutLot) ───────────────────────────
        [HttpPost("{id}/liberer-lot")]
        public async Task<IActionResult> LibererLot(int id, [FromBody] LibererLotDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.StatutLot               = "Libéré";
            demande.DateLiberationLot        = DateTime.Now;
            demande.CommentaireLiberationLot = dto.Commentaire;
            await _context.SaveChangesAsync();

            // Notifier la Production (demandeur)
            await AjouterNotification(demande.UtilisateurId,
                "Lot libéré",
                $"Le lot de votre demande {demande.Reference} a été libéré. Commentaire : {dto.Commentaire ?? "—"}",
                "success", id);

            int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var auditorIdLiberation);
            await AjouterAudit(id, auditorIdLiberation, "LIBERATION LOT",
                $"StatutLot → Libéré — {dto.Commentaire ?? "Sans commentaire"}");

            return Ok(new { message = "Lot libéré", demande });
        }

        // ── LIBÉRER MALGRÉ ANOMALIE (dérogation globale) ─────
        public record LibererMalgreAnomalieDto(int UserId, string NomUtilisateur, string? Justification);

        [HttpPost("{id}/liberer-malgre-anomalie")]
        [Authorize(Roles = "ResponsableLabo,Admin")]
        public async Task<IActionResult> LibererMalgreAnomalie(int id, [FromBody] LibererMalgreAnomalieDto dto)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return NotFound();

            demande.DerogationGlobale = true;
            if (!string.IsNullOrWhiteSpace(dto.Justification))
                demande.CommentaireLabo = dto.Justification;

            await _context.SaveChangesAsync();

            var directeurs = await _context.Utilisateurs
                .Where(u => u.Role == "DirecteurQualite").ToListAsync();
            foreach (var d in directeurs)
                await AjouterNotification(d.Id,
                    "Dérogation accordée",
                    $"La demande {demande.Reference} ({demande.TypeProduit}) a été libérée malgré anomalie. Justification : {dto.Justification ?? "—"}",
                    "warning", id);

            await AjouterAudit(id, dto.UserId,
                "DEROGATION GLOBALE",
                $"Libéré malgré anomalie — Justification : {dto.Justification ?? "—"}",
                dto.NomUtilisateur);

            return Ok(new { message = "Dérogation accordée — demande marquée DÉROGATOIRE", demande });
        }

        // ── MÉTHODE UTILITAIRE AUDIT ──────────────────────────
        private async Task AjouterAudit(int demandeId, int userId,
            string action, string details, string nomUtilisateur = "Système")
        {
            var log = new AuditLog
            {
                DemandeId = demandeId,
                UtilisateurId = userId,
                NomUtilisateur = nomUtilisateur,
                Action = action,
                Details = details,
                DateAction = DateTime.Now
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        private async Task AjouterNotification(int utilisateurId, string titre,
    string message, string type, int? demandeId = null)
        {
            var notif = new Notification
            {
                UtilisateurId = utilisateurId,
                Titre = titre,
                Message = message,
                Type = type,
                DemandeId = demandeId,
                EstLue = false,
                DateCreation = DateTime.Now
            };
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
        }
    }

}