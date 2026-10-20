using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;

namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BookingsController(AppDbContext context)
        {
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
                    TotalPrice = b.TotalPrice
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            if (dto.EndTime <= dto.StartDate)
            {
                return BadRequest("End date must be later than start date.");
            }


            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == dto.CarId);

            if(car == null)
            {
                return NotFound("Car not found");
            }
            if (!car.IsAvailable)
            {
                return BadRequest("This car is temporarily unavailable for rent");
            }


            bool bookedCar = await _context.Bookings.AnyAsync(b =>  
                b.CarId == dto.CarId && 
                b.StartDate < dto.EndTime && 
                b.EndTime > dto.StartDate);

            if (bookedCar)
            {
                return BadRequest("The car has already been reserved for the selected dates");
            }


            var duration = dto.EndTime - dto.StartDate;
            int days = (int)Math.Ceiling(duration.TotalDays);
            decimal totalPrice = days * car.DailyPrice;


            var booking = new Booking
            {
                CarId = dto.CarId,
                CustomerName = dto.CustomerName,
                StartDate = dto.StartDate,
                EndTime = dto.EndTime,
                TotalPrice = totalPrice
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
                TotalPrice = booking.TotalPrice
            };

            return Ok(responseDto);

        }


    }
}
