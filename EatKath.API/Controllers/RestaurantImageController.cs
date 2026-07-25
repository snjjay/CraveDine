using EatKath.API.DTOs.RestaurantImage;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantImageController : ControllerBase
    {
        private readonly IRestaurantImageService _service;

        public RestaurantImageController(IRestaurantImageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var image = await _service.GetByIdAsync(id);

            if (image == null)
                return NotFound();

            return Ok(image);
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(int restaurantId)
        {
            return Ok(await _service.GetByRestaurantAsync(restaurantId));
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantImageDto dto)
        {
            return Ok(await _service.CreateAsync(dto));
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateRestaurantImageDto dto)
        {
            var image = await _service.UpdateAsync(id, dto);

            if (image == null)
                return NotFound();

            return Ok(image);
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