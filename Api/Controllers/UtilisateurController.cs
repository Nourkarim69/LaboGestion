using Data.Context;
using Domain.Models;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateurController : ControllerBase
    {
        private readonly IGenericRepository<Utilisateur> _repository;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        private static readonly string[] RolesValides = {
            "Admin",
            "ResponsableLabo",
            "DirecteurQualite",
            "Technicien",
            "Production",
            "Commercial",
            "Auditeur"
        };

        public UtilisateurController(
            IGenericRepository<Utilisateur> repository,
            IConfiguration configuration,
            ApplicationDbContext db)
        {
            _repository = repository;
            _configuration = configuration;
            _db = db;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var utilisateurs = await _repository.GetAll();
            // Ne jamais retourner les mots de passe
            var result = utilisateurs.Select(u => new {
                u.Id,
                u.Nom,
                u.Prenom,
                u.Email,
                u.Role,
                u.FilialeId,
                u.EstActif
            });
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var utilisateur = await _repository.GetById(id);
            if (utilisateur == null) return NotFound();
            return Ok(new
            {
                utilisateur.Id,
                utilisateur.Nom,
                utilisateur.Prenom,
                utilisateur.Email,
                utilisateur.Role,
                utilisateur.FilialeId,
                utilisateur.EstActif
            });
        }

        [HttpGet("role/{role}")]
        [Authorize]
        public async Task<IActionResult> GetByRole(string role)
        {
            var utilisateurs = await _repository.GetAll();
            var result = utilisateurs
                .Where(u => u.Role.ToLower() == role.ToLower())
                .Select(u => new {
                    u.Id,
                    u.Nom,
                    u.Prenom,
                    u.Email,
                    u.Role,
                    u.FilialeId
                });
            return Ok(result);
        }

        [HttpGet("techniciens")]
        [Authorize]
        public async Task<IActionResult> GetTechniciens()
        {
            var utilisateurs = await _repository.GetAll();
            var result = utilisateurs
                .Where(u => u.Role == "Technicien")
                .Select(u => new {
                    u.Id,
                    u.Nom,
                    u.Prenom,
                    u.Email,
                    u.Role,
                    u.FilialeId
                });
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Utilisateur utilisateur)
        {
            if (utilisateur == null)
                return BadRequest("Données invalides");

            if (string.IsNullOrEmpty(utilisateur.Email) || string.IsNullOrEmpty(utilisateur.MotDePasse))
                return BadRequest("Email et mot de passe obligatoires");

            // Validation du rôle
            if (string.IsNullOrEmpty(utilisateur.Role) || !RolesValides.Contains(utilisateur.Role))
                return BadRequest($"Rôle invalide. Rôles acceptés : {string.Join(", ", RolesValides)}");

            // Vérifier si l'email existe déjà
            var existants = await _repository.GetAll();
            if (existants.Any(u => u.Email == utilisateur.Email))
                return BadRequest("Un compte avec cet email existe déjà");

            // Production et Commercial doivent avoir une filiale
            if ((utilisateur.Role == "Production" || utilisateur.Role == "Commercial")
                && !utilisateur.FilialeId.HasValue)
                return BadRequest("Une filiale est obligatoire pour ce rôle");

            utilisateur.MotDePasse = HashPassword(utilisateur.MotDePasse);
            await _repository.Add(utilisateur);

            return Ok(new
            {
                message = "Compte créé avec succès",
                role = utilisateur.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.MotDePasse))
                return BadRequest("Email et mot de passe obligatoires");

            var utilisateurs = await _repository.GetAll();
            var hashedPassword = HashPassword(request.MotDePasse);
            var utilisateur = utilisateurs.FirstOrDefault(u =>
                u.Email == request.Email && u.MotDePasse == hashedPassword);

            if (utilisateur == null)
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });

            if (!utilisateur.EstActif)
                return Unauthorized(new { message = "Ce compte est désactivé. Contactez votre administrateur." });

            var token = GenerateJwtToken(utilisateur);

            return Ok(new
            {
                message = "Connexion réussie",
                token,
                id = utilisateur.Id,
                nom = utilisateur.Nom,
                prenom = utilisateur.Prenom,
                email = utilisateur.Email,
                role = utilisateur.Role,
                filialeId = utilisateur.FilialeId
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUtilisateurRequest request)
        {
            var utilisateur = await _repository.GetById(id);
            if (utilisateur == null) return NotFound();

            if (!string.IsNullOrEmpty(request.Nom)) utilisateur.Nom = request.Nom;
            if (!string.IsNullOrEmpty(request.Prenom)) utilisateur.Prenom = request.Prenom;
            if (!string.IsNullOrEmpty(request.Email)) utilisateur.Email = request.Email;
            if (request.FilialeId.HasValue) utilisateur.FilialeId = request.FilialeId;

            if (!string.IsNullOrEmpty(request.Role))
            {
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (currentUserRole != "Admin")
                    return Forbid();
                if (!RolesValides.Contains(request.Role))
                    return BadRequest($"Rôle invalide. Rôles acceptés : {string.Join(", ", RolesValides)}");
                utilisateur.Role = request.Role;
            }

            if (!string.IsNullOrEmpty(request.NouveauMotDePasse))
                utilisateur.MotDePasse = HashPassword(request.NouveauMotDePasse);

            await _repository.Update(utilisateur);
            return Ok(new { message = "Utilisateur mis à jour avec succès" });
        }

        [HttpGet("seed-demo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SeedDemo()
        {
            try
            {
                var (inserted, skipped, diagnostic) = await Api.Services.DbInitializer.SeedDemoAccountsAsync(_db);
                return Ok(new
                {
                    message = $"{inserted} compte(s) créé(s), {skipped} mis à jour/existant(s).",
                    inserted,
                    skipped,
                    diagnostic,
                    motDePasse = "Demo@1234"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpPatch("{id}/desactiver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Desactiver(int id)
        {
            var utilisateur = await _repository.GetById(id);
            if (utilisateur == null) return NotFound();
            utilisateur.EstActif = false;
            await _repository.Update(utilisateur);
            return Ok(new { message = "Compte désactivé avec succès" });
        }

        [HttpPatch("{id}/reactiver")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactiver(int id)
        {
            var utilisateur = await _repository.GetById(id);
            if (utilisateur == null) return NotFound();
            utilisateur.EstActif = true;
            await _repository.Update(utilisateur);
            return Ok(new { message = "Compte réactivé avec succès" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var utilisateur = await _repository.GetById(id);
            if (utilisateur == null) return NotFound();

            // Vérifier les dépendances avant suppression
            var aDemandes = await _db.Demandes.AnyAsync(d => d.UtilisateurId == id || d.TechnicienId == id);
            var aResultats = await _db.Resultats.AnyAsync(r => r.UtilisateurId == id);
            var aNotifications = await _db.Notifications.AnyAsync(n => n.UtilisateurId == id);
            var aMessages = await _db.Messages.AnyAsync(m => m.ExpediteurId == id || m.DestinataireId == id);

            if (aDemandes || aResultats || aNotifications || aMessages)
                return Conflict(new
                {
                    message = "Impossible de supprimer cet utilisateur car il a des demandes, résultats ou messages associés. Désactivez-le à la place.",
                    peutDesactiver = true
                });

            try
            {
                await _repository.Delete(id);
                return Ok(new { message = "Utilisateur supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return Conflict(new
                {
                    message = "Impossible de supprimer cet utilisateur car il a des données liées. Désactivez-le à la place.",
                    detail = ex.InnerException?.Message ?? ex.Message,
                    peutDesactiver = true
                });
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateJwtToken(Utilisateur utilisateur)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Role, utilisateur.Role),
                new Claim(ClaimTypes.Name, utilisateur.Nom),
                new Claim("filialeId", utilisateur.FilialeId?.ToString() ?? "")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(48),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
    }

    public class UpdateUtilisateurRequest
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? FilialeId { get; set; }
        public string? NouveauMotDePasse { get; set; }
    }
}