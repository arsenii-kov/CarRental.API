using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;



namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars()
        {
            var cars = await _context.Cars.ToListAsync();
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

        [HttpPost]
        public async Task<IActionResult> CreateCar([FromBody] CreateCarDto dto)
        {
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
