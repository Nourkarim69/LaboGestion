namespace Domain.Models
{
    public class Demande
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string TypeProduit { get; set; }
        public int NombreEchantillons { get; set; }
        public string Priorite { get; set; }
        public string Statut { get; set; }
        public DateTime DateDemande { get; set; }
        public DateTime? DateRenduEstimee { get; set; }
        public DateTime? DateValidation { get; set; }
        public string CommentaireLabo { get; set; }
        public string CommentaireRejet { get; set; }
        public string ValidePar { get; set; }
        public int FilialeId { get; set; }
        public int UtilisateurId { get; set; }
        public int? TechnicienId { get; set; }
        public Utilisateur? Technicien { get; set; }
        public Utilisateur? Utilisateur { get; set; }
        public Filiale? Filiale { get; set; }

        // ── Nouveaux champs ────────────────────────────────
        public string NumeroCertificat { get; set; }
        public bool LotBloque { get; set; } = false;
        public DateTime? DateBlocage { get; set; }
        public string MotifBlocage { get; set; }
        public bool ContreAnalyseRequise { get; set; } = false;
        public int? TechnicienAssigneParId { get; set; }
        public DateTime? DateAssignation { get; set; }
        public int SlaHeures { get; set; } = 48;
        public DateTime? DateReception { get; set; }
        public DateTime? DateRenduReelle { get; set; }
        public string? CommentaireReouvrir { get; set; }
        public DateTime? DateReouverte { get; set; }

        // ── Workflow bi-niveau ──────────────────────────────────
        public string? NumeroDeLot { get; set; }
        public DateTime? DateValidationResponsable { get; set; }
        public DateTime? DateValidationDirecteur { get; set; }
        public int? ValideParResponsableId { get; set; }
        public int? ValideParDirecteurId { get; set; }
        public string? CommentaireRejetDirecteur { get; set; }
        public Utilisateur? ValideParResponsable { get; set; }
        public Utilisateur? ValideParDirecteur { get; set; }

        // ── Gestion du lot physique (indépendant du workflow) ───
        public string? StatutLot { get; set; }   // null = "Normal", "Bloqué", "Libéré"
        public string? MotifBlocageLot { get; set; }
        public DateTime? DateBlocageLot { get; set; }
        public DateTime? DateLiberationLot { get; set; }
        public string? CommentaireLiberationLot { get; set; }
        public bool DerogationGlobale { get; set; } = false;
    }
}