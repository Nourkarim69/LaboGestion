using Data.Context;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MessageController(ApplicationDbContext context) => _context = context;

        // GET /api/Message?userId={id}
        [HttpGet]
        public async Task<IActionResult> GetMesMessages([FromQuery] int userId)
        {
            var messages = await _context.Messages
                .Include(m => m.Expediteur)
                .Include(m => m.Destinataire)
                .Include(m => m.Demande)
                .Where(m => m.ExpediteurId == userId || m.DestinataireId == userId)
                .OrderByDescending(m => m.DateEnvoi)
                .Select(m => new {
                    m.Id,
                    m.Objet,
                    m.Contenu,
                    m.DateEnvoi,
                    m.Lu,
                    m.DemandeId,
                    demandeReference = m.Demande != null ? m.Demande.Reference : null,
                    expediteurId = m.ExpediteurId,
                    expediteurNom = m.Expediteur != null
                        ? (m.Expediteur.Prenom + " " + m.Expediteur.Nom).Trim() : "",
                    expediteurRole = m.Expediteur != null ? m.Expediteur.Role : "",
                    destinataireId = m.DestinataireId,
                    destinataireNom = m.Destinataire != null
                        ? (m.Destinataire.Prenom + " " + m.Destinataire.Nom).Trim() : "",
                    destinataireRole = m.Destinataire != null ? m.Destinataire.Role : "",
                    estEnvoi = m.ExpediteurId == userId
                })
                .ToListAsync();

            return Ok(messages);
        }

        // GET /api/Message/nonlus?userId={id}
        [HttpGet("nonlus")]
        public async Task<IActionResult> GetNonLus([FromQuery] int userId)
        {
            var count = await _context.Messages
                .CountAsync(m => m.DestinataireId == userId && !m.Lu);
            return Ok(new { count });
        }

        // POST /api/Message
        [HttpPost]
        public async Task<IActionResult> Envoyer([FromBody] EnvoyerMessageDto dto)
        {
            var message = new Message
            {
                ExpediteurId = dto.ExpediteurId,
                DestinataireId = dto.DestinataireId,
                Objet = dto.Objet,
                Contenu = dto.Contenu,
                DateEnvoi = DateTime.UtcNow,
                Lu = false,
                DemandeId = dto.DemandeId
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Message envoyé", id = message.Id });
        }

        // PUT /api/Message/{id}/lire
        [HttpPut("{id}/lire")]
        public async Task<IActionResult> MarquerLu(int id)
        {
            var msg = await _context.Messages.FindAsync(id);
            if (msg == null) return NotFound();
            msg.Lu = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public record EnvoyerMessageDto(
        int ExpediteurId,
        int DestinataireId,
        string Objet,
        string Contenu,
        int? DemandeId
    );
}
