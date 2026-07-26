using EatKath.API.DTOs.Deal;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealController : ControllerBase
    {
        private readonly IDealService _service;

        public DealController(IDealService service)
        {
            _service = service;
        }

        // ============================
        // Public Endpoints
        // ============================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var deals = await _service.GetAllAsync();
            return Ok(deals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var deal = await _service.GetByIdAsync(id);

            if (deal == null)
                return NotFound();

            return Ok(deal);
        }

        // ============================
        // Admin / Owner
        // ============================

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateDealDto dto)
        {
            var deal = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = deal.Id },
                deal);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDealDto dto)
        {
            var deal = await _service.UpdateAsync(id, dto);

            return Ok(deal);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}