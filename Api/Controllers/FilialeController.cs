using Domain.Models;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilialeController : ControllerBase
    {
        private readonly IGenericRepository<Filiale> _repository;

        public FilialeController(IGenericRepository<Filiale> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var filiales = await _repository.GetAll();
            return Ok(filiales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var filiale = await _repository.GetById(id);
            if (filiale == null) return NotFound();
            return Ok(filiale);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] Filiale filiale)
        {
            if (filiale == null) return BadRequest();
            await _repository.Add(filiale);
            return Ok(new { message = "Filiale ajoutée avec succès", filiale });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] Filiale filiale)
        {
            if (filiale == null) return BadRequest();
            filiale.Id = id;
            await _repository.Update(filiale);
            return Ok(new { message = "Filiale modifiée avec succès" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return Ok(new { message = "Filiale supprimée avec succès" });
        }
    }
}