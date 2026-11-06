using Data.Context;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProduitController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produits = await _context.Produits
                .Where(p => p.Actif)
                .OrderBy(p => p.FilialeId)
                .ThenBy(p => p.Nom)
                .ToListAsync();
            return Ok(produits);
        }

        [HttpGet("filiale/{filialeId}")]
        public async Task<IActionResult> GetByFiliale(int filialeId)
        {
            var produits = await _context.Produits
                .Where(p => p.FilialeId == filialeId && p.Actif)
                .OrderBy(p => p.Nom)
                .ToListAsync();
            return Ok(produits);
        }

        [HttpGet("filiale/{filialeId}/tous")]
        public async Task<IActionResult> GetAllByFiliale(int filialeId)
        {
            var produits = await _context.Produits
                .Where(p => p.FilialeId == filialeId)
                .OrderBy(p => p.Nom)
                .ToListAsync();
            return Ok(produits);
        }

        [HttpPost("copier/{sourceFilialeId}/{targetFilialeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Copier(int sourceFilialeId, int targetFilialeId)
        {
            var sourceProds = await _context.Produits
                .Where(p => p.FilialeId == sourceFilialeId && p.Actif)
                .ToListAsync();

            var existingNames = await _context.Produits
                .Where(p => p.FilialeId == targetFilialeId)
                .Select(p => p.Nom)
                .ToListAsync();

            var nouveaux = sourceProds
                .Where(sp => !existingNames.Contains(sp.Nom))
                .Select(sp => new Produit
                {
                    Nom = sp.Nom,
                    Description = sp.Description,
                    Unite = sp.Unite,
                    FilialeId = targetFilialeId,
                    Actif = true
                }).ToList();

            if (nouveaux.Count == 0)
                return Ok(new { message = "Aucun nouveau produit à copier", count = 0 });

            _context.Produits.AddRange(nouveaux);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"{nouveaux.Count} produit(s) copié(s)", count = nouveaux.Count });
        }

        [HttpPost("global")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGlobal([FromBody] ProduitGlobalRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                return BadRequest(new { message = "Le nom du produit est obligatoire" });

            var filiales = await _context.Filiales.Where(f => f.EstActive).ToListAsync();
            int created = 0;

            foreach (var filiale in filiales)
            {
                var existe = await _context.Produits
                    .AnyAsync(p => p.FilialeId == filiale.Id && p.Nom == request.Nom);
                if (existe) continue;

                _context.Produits.Add(new Produit
                {
                    Nom = request.Nom,
                    Description = request.Description ?? "",
                    Unite = request.Unite ?? "",
                    FilialeId = filiale.Id,
                    Actif = true
                });
                created++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Produit ajouté à {created} filiale(s)", count = created });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Produit produit)
        {
            if (produit == null) return BadRequest();
            produit.Actif = true;
            _context.Produits.Add(produit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit ajouté", produit });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Produit produit)
        {
            var existing = await _context.Produits.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Nom = produit.Nom;
            existing.Description = produit.Description;
            existing.Unite = produit.Unite;
            existing.Actif = produit.Actif;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit modifié", existing });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit == null) return NotFound();
            produit.Actif = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit désactivé" });
        }
    }

    public record ProduitGlobalRequest(string Nom, string? Description, string? Unite);
}