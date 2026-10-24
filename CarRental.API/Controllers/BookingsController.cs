using CarRental.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using CarRental.API.Services;

namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IValidator<CreateBookingDto> _validator;
        private readonly IValidator<UpdateBookingStatusDto> _updateValidator;
        private readonly IBookingService _bookingService;
        public BookingsController(IValidator<CreateBookingDto> validator, IValidator<UpdateBookingStatusDto> updateValidator, IBookingService bookingService)
        {
            _validator = validator;
            _updateValidator = updateValidator;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();

            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
            {
                return NotFound("Reservation not found.");
            }

            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            // Валидация входного DTO
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            // Вызов сервиса
            var result = await _bookingService.CreateBookingAsync(dto);

            if (result == null)
            {
                return BadRequest("Unable to create booking. The car is unavailable or dates overlap.");
            }


            return CreatedAtAction(nameof(GetBookingById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var result = await _bookingService.UpdateBookingStatusAsync(id, dto.Status);

            if (!result)
            {
                return BadRequest("Unable to update status. The booking was not found or has already been completed/cancelled.");
            }

            return NoContent();



        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.CancelBookingAsync(id);

            if (!result)
            {
                return NotFound("Booking not found or cannot be cancelled.");
            }

            return NoContent();
        }
    }
}
