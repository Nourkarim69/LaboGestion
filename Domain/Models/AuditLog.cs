namespace Domain.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? DemandeId { get; set; }
        public int UtilisateurId { get; set; }
        public string? NomUtilisateur { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public DateTime DateAction { get; set; } = DateTime.Now;
    }
}