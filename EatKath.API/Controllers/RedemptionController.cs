using EatKath.API.DTOs.Redemption;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedemptionController : ControllerBase
    {
        private readonly IRedemptionService _service;

        public RedemptionController(IRedemptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Redeem([FromBody] CreateRedemptionDto dto)
        {
            var result = await _service.RedeemAsync(dto);

            return Ok(result);
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            return Ok(await _service.GetMyHistoryAsync());
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetRestaurantRedemptions(int restaurantId)
        {
            return Ok(await _service.GetRestaurantRedemptionsAsync(restaurantId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}