using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EatKath.API.Controllers
{
    // Surprise Bag listings: public browsing and owner/admin management.
    // (Reservations are not part of this controller.)
    [ApiController]
    [Route("api/[controller]")]
    public class SurpriseBagController : ControllerBase
    {
        private readonly ISurpriseBagService _service;

        public SurpriseBagController(ISurpriseBagService service)
        {
            _service = service;
        }

        // Today's open and upcoming bags of an active restaurant (sold-out
        // bags included, with IsSoldOut).
        [AllowAnonymous]
        [HttpGet("restaurant/{restaurantId:int}")]
        public async Task<IActionResult> GetByRestaurant(int restaurantId)
        {
            return Ok(await _service.GetPublicByRestaurantAsync(restaurantId));
        }

        // 404 unless the bag is publicly visible.
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bag = await _service.GetPublicByIdAsync(id);

            if (bag == null)
                return NotFound();

            return Ok(bag);
        }

        // All of the owner's bags, including inactive and past ones.
        [Authorize(Roles = "Owner")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBags([FromQuery] int? restaurantId)
        {
            var ownerId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return Ok(await _service.GetByOwnerAsync(ownerId, restaurantId));
        }

        // One bag for its owner (or an Admin), whatever its state.
        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("my/{id:int}")]
        public async Task<IActionResult> GetMyBag(int id)
        {
            var bag = await _service.GetManagedByIdAsync(id);

            if (bag == null)
                return NotFound();

            return Ok(bag);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSurpriseBagDto dto)
        {
            var bag = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetMyBag), new { id = bag.Id }, bag);
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSurpriseBagDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        // Soft delete: deactivates the bag; existing reservations are kept.
        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var deactivated = await _service.DeactivateAsync(id);

            if (!deactivated)
                return NotFound();

            return NoContent();
        }
    }
}
