namespace Domain.Models
{
    public class Resultat
    {
        public int Id { get; set; }
        public double Valeur { get; set; }
        public bool EstAnomal { get; set; }
        public DateTime DateSaisie { get; set; }
        public string Statut { get; set; } = string.Empty;
        public int EchantillonId { get; set; }
        public Echantillon? Echantillon { get; set; }
        public int AnalyseId { get; set; }
        public Analyse? Analyse { get; set; }
        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
        public int? DemandeId { get; set; }
        public Demande? Demande { get; set; }
        public bool Derogation { get; set; } = false;
        public string? JustificationDerogation { get; set; }
    }
}