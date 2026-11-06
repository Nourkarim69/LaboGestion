using Domain.Models;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleAccesController : ControllerBase
    {
        private readonly IGenericRepository<CleAcces> _repository;

        public CleAccesController(IGenericRepository<CleAcces> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var cles = await _repository.GetAll();
            return Ok(cles);
        }

        [HttpPost("generer")]
        [Authorize]
        public async Task<IActionResult> Generer([FromBody] GenererCleRequest request)
        {
            int? filialeId = null;
            if (request.Role == "Production" && request.FilialeId.HasValue && request.FilialeId.Value > 0)
            {
                filialeId = request.FilialeId.Value;
            }

            var cle = new CleAcces
            {
                Cle = GenerateCle(request.Role),
                Role = request.Role,
                FilialeId = filialeId,
                EstUtilisee = false,
                DateCreation = DateTime.Now,
                DateExpiration = DateTime.Now.AddHours(48),
                CreePar = request.CreePar
            };

            await _repository.Add(cle);
            return Ok(new
            {
                message = "Clé générée avec succès",
                cle = cle.Cle,
                role = cle.Role,
                expiration = cle.DateExpiration
            });
        }

        [HttpPost("valider")]
        public async Task<IActionResult> Valider([FromBody] ValiderCleRequest request)
        {
            var cles = await _repository.GetAll();
            var cle = cles.FirstOrDefault(c =>
                c.Cle == request.Cle &&
                !c.EstUtilisee &&
                c.DateExpiration > DateTime.Now);

            if (cle == null)
                return BadRequest(new { message = "Clé invalide, expirée ou déjà utilisée" });

            return Ok(new
            {
                message = "Clé valide",
                role = cle.Role,
                filialeId = cle.FilialeId,
                cleId = cle.Id
            });
        }

        [HttpPut("{id}/utiliser")]
        public async Task<IActionResult> Utiliser(int id)
        {
            var cle = await _repository.GetById(id);
            if (cle == null) return NotFound();
            cle.EstUtilisee = true;
            await _repository.Update(cle);
            return Ok(new { message = "Clé marquée comme utilisée" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return Ok(new { message = "Clé supprimée" });
        }

        private string GenerateCle(string role)
        {
            var prefix = role switch
            {
                "Admin" => "ADM",
                "Technicien" => "TECH",
                "Production" => "PROD",
                _ => "USR"
            };
            var random = new Random();
            var suffix = random.Next(100000, 999999).ToString();
            return $"{prefix}-{suffix}";
        }
    }

    public class GenererCleRequest
    {
        public string Role { get; set; } = string.Empty;
        public int? FilialeId { get; set; }
        public int CreePar { get; set; }
    }

    public class ValiderCleRequest
    {
        public string Cle { get; set; } = string.Empty;
    }
}