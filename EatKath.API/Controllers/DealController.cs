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
        private readonly IDealService _dealService;

        public DealController(IDealService dealService)
        {
            _dealService = dealService;
        }

        // Public - Anyone can view all deals
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealDto>>> GetAll()
        {
            var deals = await _dealService.GetAllAsync();
            return Ok(deals);
        }

        // Public - Anyone can view a single deal
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<DealDto>> GetById(int id)
        {
            var deal = await _dealService.GetByIdAsync(id);

            if (deal == null)
                return NotFound();

            return Ok(deal);
        }

        // Protected - Only Admin and Owner can create deals
        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<ActionResult<DealDto>> Create(CreateDealDto dto)
        {
            var deal = await _dealService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = deal.Id },
                deal);
        }

        // Protected - Only Admin and Owner can update deals
        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{id}")]
        public async Task<ActionResult<DealDto>> Update(int id, UpdateDealDto dto)
        {
            try
            {
                var deal = await _dealService.UpdateAsync(id, dto);
                return Ok(deal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Protected - Only Admin and Owner can delete deals
        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _dealService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}