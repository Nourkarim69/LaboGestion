using Domain.Interface;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EchantillonController : ControllerBase
    {
        private readonly IGenericRepository<Echantillon> _repository;

        public EchantillonController(IGenericRepository<Echantillon> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var echantillons = await _repository.GetAll();
            return Ok(echantillons);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var echantillon = await _repository.GetById(id);
            if (echantillon == null) return NotFound();
            return Ok(echantillon);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Echantillon echantillon)
        {
            if (echantillon == null) return BadRequest();
            await _repository.Add(echantillon);
            return Ok(new { message = "Echantillon ajouté avec succès", echantillon });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Echantillon echantillon)
        {
            if (echantillon == null) return BadRequest();
            echantillon.Id = id;
            await _repository.Update(echantillon);
            return Ok(new { message = "Echantillon modifié avec succès" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return Ok(new { message = "Echantillon supprimé avec succès" });
        }
    }
}