namespace Domain.Models
{
    public class Analyse
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public double ValeurMin { get; set; }
        public double ValeurMax { get; set; }
        public string Unite { get; set; }
    }
}