using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using CarRental.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace CarRental.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IValidator<CreateCarDto> _validator;
        private readonly IValidator<UpdateCarDto> _updateValidator;

        public CarsController(ICarService carService, IValidator<CreateCarDto> validator, IValidator<UpdateCarDto> updateValidator)
        {
            _carService = carService;
            _validator = validator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars([FromQuery] CarQueryParametersDto query)
        {
            if (query.PageNumber < 1 || query.PageSize < 1)
            {
                return BadRequest("PageNumber and PageSize must be greater than 0.");
            }

            var cars = await _carService.GetAllCarsAsync(query);
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await _carService.GetByIdAsync(id);

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
            
            var availableCars = await _carService.GetAvailableCarsAsync(startDate, endDate);
            
            return Ok(availableCars);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCar([FromBody] CreateCarDto dto)
        {
            // Валидация
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var car = await _carService.CreateCarAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCar([FromBody] UpdateCarDto dto, int id)
        {
            // Валидация
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ToDictionary());
            }

            var result = await _carService.UpdateCarAsync(id, dto);

            if (!result) 
            {
                return NotFound("The car was not found");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var result = await _carService.DeleteCarAsync(id);

            if (!result)
            {
                return NotFound("The car was not found");
            }

            return Ok("The car has been successfully removed!");
        }
    }
}
