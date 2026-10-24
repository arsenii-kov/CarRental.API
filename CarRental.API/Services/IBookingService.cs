using CarRental.API.DTOs;
using CarRental.API.Models;

namespace CarRental.API.Services;

public interface IBookingService
{
    Task<BookingDto?> CreateBookingAsync(CreateBookingDto dto);
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task<bool> UpdateBookingStatusAsync(int id, BookingStatus newStatus);
    Task<bool> CancelBookingAsync(int id);
}