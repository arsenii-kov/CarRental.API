using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRental.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingDto?> CreateBookingAsync(CreateBookingDto dto)
        {
            // Поиск авто
            var car = await _context.Cars.FindAsync(dto.CarId);

            if (car == null || !car.IsAvailable)
            {
                return null;
            }

            // Проверка на пересечение дат
            bool bookedCar = await _context.Bookings.AnyAsync(b =>
                b.CarId == dto.CarId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartDate < dto.EndTime &&
                b.EndTime > dto.StartDate);

            if (bookedCar)
            {
                return null;
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

            return responseDto;
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync(BookingQueryParametersDto query)
        {
            var queryable = _context.Bookings.AsNoTracking().AsQueryable();

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(b => b.Status == query.Status.Value); 
            }

            if (query.CarId.HasValue)
            {
                queryable = queryable.Where(b => b.CarId == query.CarId.Value);
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(b => b.StartDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(b => b.EndTime <= query.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.CustomerName))
            {
                queryable = queryable.Where(b => b.CustomerName.Contains(query.CustomerName));
            }

            queryable = queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);

            return await queryable
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    CarId = b.CarId,
                    CustomerName = b.CustomerName,
                    StartDate = b.StartDate,
                    EndTime = b.EndTime,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    IncludeInsurance = b.IncludeInsurance
                })
                .ToListAsync();
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return null;
            }

            return new BookingDto
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
        }

        public async Task<bool> UpdateBookingStatusAsync(int id, BookingStatus newStatus)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return false;
            }

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                return false;
            }

            booking.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return false;
            }

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed || booking.StartDate <= DateTime.UtcNow)
            {
                return false;
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
