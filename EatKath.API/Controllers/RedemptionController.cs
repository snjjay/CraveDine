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

        // Customer claims a walk-in offer. No Reservation is created.
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Redeem(
            [FromBody] CreateRedemptionDto dto)
        {
            var result = await _service.RedeemAsync(dto);

            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}/cancel-mine")]
        public async Task<IActionResult> CancelMine(int id)
        {
            return Ok(await _service.CancelMyRedemptionAsync(id));
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            return Ok(await _service.CancelRedemptionAsync(id));
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            return Ok(await _service.GetMyHistoryAsync());
        }

        [Authorize(Roles = "Admin,Owner")]
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetRestaurantRedemptions(
            int restaurantId)
        {
            return Ok(
                await _service.GetRestaurantRedemptionsAsync(
                    restaurantId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteRedemption(
            int id,
            [FromBody] CompleteRedemptionDto dto)
        {
            var result =
                await _service.CompleteRedemptionAsync(
                    id,
                    dto);

            return Ok(result);
        }
    }
}