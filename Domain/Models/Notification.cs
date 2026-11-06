namespace Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "info", "success", "warning", "danger"
        public bool EstLue { get; set; } = false;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
        public int? DemandeId { get; set; }
        public Demande? Demande { get; set; }
    }
}