namespace Domain.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? FilialeId { get; set; }
        public bool EstActif { get; set; } = true;
        public Filiale? Filiale { get; set; }
    }
}