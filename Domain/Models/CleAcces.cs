namespace Domain.Models
{
    public class CleAcces
    {
        public int Id { get; set; }
        public string Cle { get; set; }
        public string Role { get; set; } // Admin, Technicien, Client
        public int? FilialeId { get; set; }
        public Filiale Filiale { get; set; }
        public bool EstUtilisee { get; set; } = false;
        public DateTime DateCreation { get; set; }
        public DateTime? DateExpiration { get; set; }
        public int CreePar { get; set; }
    }
}