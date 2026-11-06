namespace Domain.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ExpediteurId { get; set; }
        public Utilisateur? Expediteur { get; set; }
        public int DestinataireId { get; set; }
        public Utilisateur? Destinataire { get; set; }
        public string Objet { get; set; } = "";
        public string Contenu { get; set; } = "";
        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
        public bool Lu { get; set; } = false;
        public int? DemandeId { get; set; }
        public Demande? Demande { get; set; }
    }
}
