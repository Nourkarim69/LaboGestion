using Domain.Interface;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly IGenericRepository<AuditLog> _repository;

        public AuditLogController(IGenericRepository<AuditLog> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _repository.GetAll();
            return Ok(logs.OrderByDescending(l => l.DateAction));
        }

        [HttpGet("demande/{demandeId}")]
        public async Task<IActionResult> GetByDemande(int demandeId)
        {
            var logs = await _repository.GetAll();
            return Ok(logs.Where(l => l.DemandeId == demandeId)
                         .OrderByDescending(l => l.DateAction));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AuditLog log)
        {
            log.DateAction = DateTime.Now;
            await _repository.Add(log);
            return Ok(log);
        }
    }
}