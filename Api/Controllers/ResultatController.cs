using Domain.Models;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Data.Context;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResultatController : ControllerBase
    {
        private readonly IGenericRepository<Resultat> _repository;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ResultatController(
            IGenericRepository<Resultat> repository,
            ApplicationDbContext context,
            EmailService emailService)
        {
            _repository = repository;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var resultats = await _context.Resultats.ToListAsync();
            return Ok(resultats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resultat = await _repository.GetById(id);
            if (resultat == null) return NotFound();
            return Ok(resultat);
        }

        [HttpGet("demande/{demandeId}")]
        public async Task<IActionResult> GetByDemande(int demandeId)
        {
            var resultats = await _context.Resultats
                .Where(r => r.DemandeId == demandeId)
                .ToListAsync();
            return Ok(resultats);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Resultat resultat)
        {
            if (resultat == null) return BadRequest();

            var analyse = await _context.Analyses.FindAsync(resultat.AnalyseId);
            if (analyse != null)
            {
                resultat.EstAnomal = resultat.Valeur < analyse.ValeurMin ||
                                     resultat.Valeur > analyse.ValeurMax;
            }

            resultat.DateSaisie = DateTime.Now;
            resultat.Statut = "Brouillon";

            await _repository.Add(resultat);

            if (resultat.Derogation && resultat.DemandeId.HasValue)
            {
                var demande = await _context.Demandes.FindAsync(resultat.DemandeId.Value);
                if (demande != null && !demande.DerogationGlobale)
                {
                    demande.DerogationGlobale = true;
                    await _context.SaveChangesAsync();
                }
            }

            if (resultat.EstAnomal && analyse != null)
            {
                // Récupérer la demande pour obtenir la référence réelle et le technicien assigné
                Demande? demande = null;
                if (resultat.DemandeId.HasValue)
                    demande = await _context.Demandes.FindAsync(resultat.DemandeId.Value);

                var reference = demande?.Reference
                    ?? (resultat.DemandeId.HasValue ? $"DEM-{resultat.DemandeId}" : $"ECH-{resultat.EchantillonId}");

                // Notifier les admins et le responsable labo
                var destinatairesAlerte = await _context.Utilisateurs
                    .Where(u => u.Role == "Admin" || u.Role == "ResponsableLabo")
                    .ToListAsync();

                foreach (var dest in destinatairesAlerte)
                {
                    if (dest.Email != null)
                    {
                        await _emailService.NotifierAnomalieDetectee(
                            dest.Email,
                            $"{dest.Prenom} {dest.Nom}",
                            reference,
                            analyse.Nom,
                            resultat.Valeur,
                            analyse.Unite,
                            analyse.ValeurMin,
                            analyse.ValeurMax
                        );
                    }

                    _context.Notifications.Add(new Notification
                    {
                        UtilisateurId = dest.Id,
                        Titre = "Anomalie détectée",
                        Message = $"Anomalie sur {analyse.Nom} — Demande {reference} — Valeur : {resultat.Valeur} {analyse.Unite} (Norme : {analyse.ValeurMin}–{analyse.ValeurMax} {analyse.Unite})",
                        Type = "danger",
                        DemandeId = resultat.DemandeId,
                        DateCreation = DateTime.Now,
                        EstLue = false
                    });
                }

                // Notifier uniquement le technicien assigné à la demande (pas tous les techniciens)
                if (demande?.TechnicienId.HasValue == true)
                {
                    var techAssigne = await _context.Utilisateurs.FindAsync(demande.TechnicienId.Value);
                    if (techAssigne?.Email != null)
                    {
                        await _emailService.NotifierAnomalieDetectee(
                            techAssigne.Email,
                            $"{techAssigne.Prenom} {techAssigne.Nom}",
                            reference,
                            analyse.Nom,
                            resultat.Valeur,
                            analyse.Unite,
                            analyse.ValeurMin,
                            analyse.ValeurMax
                        );
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Résultat ajouté avec succès",
                estAnomal = resultat.EstAnomal,
                resultat
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Resultat resultat)
        {
            if (resultat == null) return BadRequest();
            resultat.Id = id;
            await _repository.Update(resultat);
            return Ok(new { message = "Résultat modifié avec succès" });
        }

        public record DerogationRequest(bool Derogation, string? JustificationDerogation);

        [HttpPatch("{id}/derogation")]
        [Authorize(Roles = "ResponsableLabo,Admin")]
        public async Task<IActionResult> UpdateDerogation(int id, [FromBody] DerogationRequest dto)
        {
            var resultat = await _context.Resultats.FindAsync(id);
            if (resultat == null) return NotFound();

            resultat.Derogation = dto.Derogation;
            resultat.JustificationDerogation = dto.JustificationDerogation;
            await _context.SaveChangesAsync();

            if (resultat.DemandeId.HasValue)
            {
                var demande = await _context.Demandes.FindAsync(resultat.DemandeId.Value);
                if (demande != null)
                {
                    demande.DerogationGlobale = await _context.Resultats
                        .AnyAsync(r => r.DemandeId == resultat.DemandeId && r.Derogation);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Dérogation mise à jour", resultat });
        }

        [HttpPut("{id}/valider")]
        public async Task<IActionResult> Valider(int id)
        {
            var resultat = await _repository.GetById(id);
            if (resultat == null) return NotFound();
            resultat.Statut = "Validé";
            await _repository.Update(resultat);
            return Ok(new { message = "Résultat validé avec succès" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ResponsableLabo")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return Ok(new { message = "Résultat supprimé avec succès" });
        }
    }
}