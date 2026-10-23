using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateBookingDto> _validator;
        public BookingsController(AppDbContext context, IValidator<CreateBookingDto> validator)
        {
            _validator = validator;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    CarId = b.CarId,
                    CustomerName = b.CustomerName,
                    StartDate = b.StartDate,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            return Ok(bookings);
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

            // Поиск авто
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == dto.CarId);

            if(car == null)
            {
                return NotFound("Car not found");
            }
            if (!car.IsAvailable)
            {
                return BadRequest("This car is temporarily unavailable for rent");
            }

            // Проверка на пересечение дат
            bool bookedCar = await _context.Bookings.AnyAsync(b =>
                b.CarId == dto.CarId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartDate < dto.EndTime &&
                b.EndTime > dto.StartDate);

            if (bookedCar)
            {
                return BadRequest("The car has already been reserved for the selected dates");
            }

            // Расчет стоимости
            var duration = dto.EndTime - dto.StartDate;
            int days = (int)Math.Ceiling(duration.TotalDays);

            // Скидки
            decimal discountRate = 0m;
            if (days >= 10)
            {
                discountRate = 0.10m;
            }
            else if (days >= 4)
            {
                discountRate = 0.05m;
            }

            decimal basePrice = days * car.DailyPrice;
            decimal discountedPrice = basePrice * (1 - discountRate);

            // Страховка
            decimal insuranceCost = 0m;

            if (dto.IncludeInsurance == true)
            {
                insuranceCost = days * 50m;
            }



            decimal totalPrice = discountedPrice + insuranceCost;

            var booking = new Booking
            {
                CarId = dto.CarId,
                CustomerName = dto.CustomerName,
                StartDate = dto.StartDate,
                EndTime = dto.EndTime,
                TotalPrice = totalPrice,
                IncludeInsurance = dto.IncludeInsurance,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var responseDto = new BookingDto
            {
                Id = booking.Id,
                CarId = booking.CarId,
                CustomerName = booking.CustomerName,
                StartDate = booking.StartDate,
                EndTime = booking.EndTime,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                IncludeInsurance = booking.IncludeInsurance
            };

            return Ok(responseDto);
        }

        [HttpDelete ("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound("No reservation was found for this ID");
            }

            if (booking.Status == BookingStatus.Cancelled)
            {
                return BadRequest("This booking is already cancelled.");
            }

            if (booking.Status == BookingStatus.Completed)
            {
                return BadRequest("Completed bookings cannot be cancelled");
            }

            if (booking.StartDate <= DateTime.Now)
            {
                return BadRequest("A lease that has begun or has already ended cannot be canceled");
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
