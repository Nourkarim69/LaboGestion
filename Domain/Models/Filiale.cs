namespace Domain.Models
{
    public class Filiale
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Produit { get; set; }
        public bool EstActive { get; set; } = true;
    }
}