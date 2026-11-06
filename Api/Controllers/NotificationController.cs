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
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("utilisateur/{utilisateurId}")]
        public async Task<IActionResult> GetByUtilisateur(int utilisateurId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UtilisateurId == utilisateurId)
                .OrderByDescending(n => n.DateCreation)
                .Take(20)
                .Select(n => new {
                    n.Id,
                    n.Titre,
                    n.Message,
                    n.Type,
                    n.EstLue,
                    n.DateCreation,
                    n.DemandeId
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("nonlues/{utilisateurId}")]
        public async Task<IActionResult> GetNonLues(int utilisateurId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UtilisateurId == utilisateurId && !n.EstLue);

            return Ok(new { count });
        }

        [HttpPut("lire/{id}")]
        public async Task<IActionResult> MarquerLue(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();
            notification.EstLue = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("toutlire/{utilisateurId}")]
        public async Task<IActionResult> MarquerToutesLues(int utilisateurId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UtilisateurId == utilisateurId && !n.EstLue)
                .ToListAsync();

            notifications.ForEach(n => n.EstLue = true);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("creer")]
        public async Task<IActionResult> Creer([FromBody] NotificationRequest request)
        {
            if (request.UtilisateurId <= 0)
                return BadRequest("UtilisateurId requis");

            var notification = new Notification
            {
                Titre = request.Titre,
                Message = request.Message,
                Type = request.Type ?? "info",
                UtilisateurId = request.UtilisateurId,
                DemandeId = request.DemandeId,
                DateCreation = DateTime.Now,
                EstLue = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { id = notification.Id });
        }
    }

    public class NotificationRequest
    {
        public string Titre { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Type { get; set; }
        public int UtilisateurId { get; set; }
        public int? DemandeId { get; set; }
    }
}