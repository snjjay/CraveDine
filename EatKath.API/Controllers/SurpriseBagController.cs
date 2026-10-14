using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Interfaces;
using EatKath.API.Services;
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

        // Open and upcoming bags across all active restaurants (e.g. the
        // home page's featured bags). Reservable bags come first; take is
        // clamped to 1-100.
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int take = SurpriseBagService.DefaultPublicTake)
        {
            return Ok(await _service.GetPublicAsync(take));
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

        // Uploads or replaces the bag's optional image (multipart/form-data,
        // field "file"; JPG, PNG or WEBP up to 5 MB). Allowed for inactive
        // bags and when the pickup details are locked.
        [Authorize(Roles = "Admin,Owner")]
        [HttpPost("{id:int}/image")]
        public async Task<IActionResult> UploadImage(int id, [FromForm] UploadSurpriseBagImageDto dto)
        {
            var imageUrl = await _service.UploadImageAsync(id, dto.File);

            return Ok(new { ImageUrl = imageUrl });
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpDelete("{id:int}/image")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            await _service.DeleteImageAsync(id);

            return NoContent();
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
