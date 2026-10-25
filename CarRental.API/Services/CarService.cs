using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.API.Services
{
    public class CarService : ICarService
    {
        private readonly AppDbContext _context;

        public CarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarDto>> GetAllCarsAsync(CarQueryParametersDto query)
        {

            var carsQuery = _context.Cars.AsQueryable();

            if (!string.IsNullOrEmpty(query.Brand))
            {
                carsQuery = carsQuery.Where(c => c.Brand == query.Brand);
            }

            if (query.MinPrice.HasValue)
            {
                carsQuery = carsQuery.Where(c => c.DailyPrice >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                carsQuery = carsQuery.Where(c => c.DailyPrice <= query.MaxPrice.Value);
            }

            carsQuery = carsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            var cars = await carsQuery
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

            return cars;
        }


        public async Task<CarDto?> GetByIdAsync(int id)
        {
            var car = await _context.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return null;
            }
            return new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                DailyPrice = car.DailyPrice,
                IsAvailable = car.IsAvailable
            };
        }

        public async Task<IEnumerable<CarDto>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
        {
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

            return availableCars;
        }

        public async Task<CarDto> CreateCarAsync(CreateCarDto dto)
        {
            var car = new Car
            {
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                DailyPrice = dto.DailyPrice,
                IsAvailable = true
            };
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            var responseDto = new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                DailyPrice = car.DailyPrice,
                IsAvailable = car.IsAvailable,
            };

            return responseDto;
        }

        public async Task<bool> UpdateCarAsync(int id, UpdateCarDto dto)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return false;
            }

            car.DailyPrice = dto.DailyPrice;
            car.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
            {
                return false;
            }
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
