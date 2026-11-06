namespace Domain.Models
{
    public class Echantillon
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime DatePrelevement { get; set; }
        public DateTime? DateReception { get; set; }
        public string Statut { get; set; }
        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        // Traçabilité métier
        public int? DemandeId { get; set; }
        public string NumeroLot { get; set; }
        public decimal? TemperatureTransport { get; set; }
        public int? DelaiTransportHeures { get; set; }
        public string PrelevePar { get; set; }
        public string ReceptionPar { get; set; }
        public string ConditionConservation { get; set; }
        public string Observations { get; set; }
    }
}