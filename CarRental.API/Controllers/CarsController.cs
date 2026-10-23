using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using FluentValidation;



namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateCarDto> _validator;

        public CarsController(AppDbContext context, IValidator<CreateCarDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars(
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber < 1 ||  pageSize < 1)
            {
                return BadRequest("PageNumber and PageSize must be greater than 0.");
            }

            var query = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(c => c.Brand == brand);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.DailyPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.DailyPrice <= maxPrice.Value);
            }

            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);


            var cars = await query
                .Select(c => new CarDto 
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    DailyPrice = c.DailyPrice,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync();
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound("The car was not found");
            }
            return Ok(car);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCars(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                return BadRequest("End date must be later than start date");
            }

            var availableCars = await _context.Cars
                .Where(c => c.IsAvailable)
                .Where(c => !c.Bookings.Any(b =>
                    b.StartDate < endDate &&
                    b.Status != BookingStatus.Cancelled &&
                    b.EndTime > startDate))
                .Select(c => new CarDto 
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    DailyPrice = c.DailyPrice,
                    IsAvailable = c.IsAvailable
                })
                .ToListAsync();

            return Ok(availableCars);

        }

        [HttpPost]
        public async Task<IActionResult> CreateCar([FromBody] CreateCarDto dto)
        {
            // Валидация
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }


            var car = new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                DailyPrice = dto.DailyPrice
            };
            _context.Cars.Add(car);
            await _context.SaveChangesAsync ();

            return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar([FromBody] UpdateCarDto dto, int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound("The car was not found");
            }
            car.DailyPrice = dto.DailyPrice;
            car.IsAvailable = dto.IsAvailable;

            await _context .SaveChangesAsync();
            return Ok(car);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return NotFound("The car was not found");
            }
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok("The car has been successfully removed!");
        }
    }
}
