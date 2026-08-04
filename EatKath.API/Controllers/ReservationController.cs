using System.Security.Claims;
using EatKath.API.DTOs.Reservation;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        // =====================================
        // Admin - Get All Reservations
        // =====================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _service.GetAllAsync();

            return Ok(reservations);
        }

        // =====================================
        // Owner - My Reservations
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpGet("owner")]
        public async Task<IActionResult> GetOwnerReservations()
        {
            var ownerId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reservations =
                await _service.GetOwnerReservationsAsync(ownerId);

            return Ok(reservations);
        }

        // =====================================
        // Customer - My Reservations
        // =====================================

        [Authorize(Roles = "Customer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var reservations =
                await _service.GetMyReservationsAsync(userId);

            return Ok(reservations);
        }



        // =====================================
        // Get Reservation
        // =====================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _service.GetByIdAsync(id);

            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }

        // =====================================
        // Customer Creates Reservation
        // =====================================

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> Create(
    [FromBody] CreateReservationDto dto)
        {
            var reservation =
                await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = reservation.Id },
                reservation);
        }

        // =====================================
        // Confirm Reservation
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            var updated = await _service.ConfirmReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // Reject Reservation
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var updated = await _service.RejectReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // Customer Arrived
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/arrived")]
        public async Task<IActionResult> Arrived(int id)
        {
            var updated = await _service.ArriveReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // Reservation Completed
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/completed")]
        public async Task<IActionResult> Completed(int id)
        {
            var updated = await _service.CompleteReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // No Show
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/no-show")]
        public async Task<IActionResult> NoShow(int id)
        {
            var updated = await _service.NoShowReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // Cancel Reservation
        // =====================================

        [Authorize(Roles = "Owner")]
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var updated = await _service.CancelReservationAsync(id);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // =====================================
        // Delete Reservation
        // =====================================

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}