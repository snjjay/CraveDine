using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    // Customer Surprise Bag reservations (pay at pickup) and the restaurant's
    // handling of them. Business-rule errors are 400; missing or
    // inaccessible reservations and restaurants are 404.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurpriseBagReservationController : ControllerBase
    {
        private readonly ISurpriseBagReservationService _service;

        public SurpriseBagReservationController(ISurpriseBagReservationService service)
        {
            _service = service;
        }

        // ============================
        // Customer
        // ============================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSurpriseBagReservationDto dto)
        {
            var reservation = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            return Ok(await _service.GetMyAsync());
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id:int}/cancel-mine")]
        public async Task<IActionResult> CancelMine(int id)
        {
            return Ok(await _service.CancelMineAsync(id));
        }

        // The customer, the restaurant's owner or an Admin.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);

            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }

        // ============================
        // Restaurant owner / Admin
        // ============================

        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("restaurant/{restaurantId:int}")]
        public async Task<IActionResult> GetForRestaurant(
            int restaurantId,
            [FromQuery] DateOnly? pickupDate,
            [FromQuery] SurpriseBagReservationStatus? status)
        {
            return Ok(await _service.GetForRestaurantAsync(restaurantId, pickupDate, status));
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            return Ok(await _service.CancelAsync(id));
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("collect")]
        public async Task<IActionResult> Collect([FromBody] CollectSurpriseBagReservationDto dto)
        {
            return Ok(await _service.CollectAsync(dto));
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id:int}/no-show")]
        public async Task<IActionResult> NoShow(int id)
        {
            return Ok(await _service.MarkNoShowAsync(id));
        }
    }
}
