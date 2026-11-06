namespace Domain.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public string Unite { get; set; }
        public bool Actif { get; set; } = true;
        public int FilialeId { get; set; }
        public Filiale Filiale { get; set; }
    }
}