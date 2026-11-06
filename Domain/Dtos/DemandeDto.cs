namespace Domain.Dtos
{
    public class CreateDemandeDto
    {
        public string Description { get; set; } = string.Empty;
        public int FilialeId { get; set; }
        public string TypeProduit { get; set; } = string.Empty;
        public int NombreEchantillons { get; set; } = 1;
        public string Priorite { get; set; } = "Normal";
        public int UtilisateurId { get; set; }
        public int SlaHeures { get; set; } = 48;
        public string? NumeroDeLot { get; set; }
    }

    public class BloquerLotNouveauDto
    {
        public string Motif { get; set; } = string.Empty;
    }

    public class LibererLotDto
    {
        public string? Commentaire { get; set; }
    }

    public class ReceptionDto
    {
        public DateTime DateRenduEstimee { get; set; }
        public int? TechnicienId { get; set; }
        public string? Commentaire { get; set; }
    }

    public class TerminerDto
    {
        public string? Commentaire { get; set; }
    }

    public class ApprobationDto
    {
        public int AdminId { get; set; }
        public string? NomAdmin { get; set; }
    }

    public class RejetDto
    {
        public int AdminId { get; set; }
        public string? NomAdmin { get; set; }
        public string? Motif { get; set; }
    }

    public class AssignationDto
    {
        public int TechnicienId { get; set; }
        public int AssigneParId { get; set; }
        public string? NomAssignePar { get; set; }
    }

    public class BlocageDto
    {
        public int UserId { get; set; }
        public string? NomUtilisateur { get; set; }
        public string? Motif { get; set; }
    }

    public class BaseActionDto
    {
        public int UserId { get; set; }
        public string? NomUtilisateur { get; set; }
    }

    public class ReouvrirDto
    {
        public int UserId { get; set; }
        public string? NomUtilisateur { get; set; }
        public string Commentaire { get; set; } = string.Empty;
    }

    public class ValidationResponsableDto
    {
        public int ResponsableId { get; set; }
        public string? NomResponsable { get; set; }
        public string? Commentaire { get; set; }
    }

    public class ValidationDirecteurDto
    {
        public int DirecteurId { get; set; }
        public string? NomDirecteur { get; set; }
        public string? Commentaire { get; set; }
    }

    public class RejetDirecteurDto
    {
        public int DirecteurId { get; set; }
        public string? NomDirecteur { get; set; }
        public string Motif { get; set; } = string.Empty;
    }
}