using CarRental.API.DTOs;
using CarRental.API.Models;

namespace CarRental.API.Services;

public interface IBookingService
{
    Task<BookingDto?> CreateBookingAsync(CreateBookingDto dto, int userId);
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync(BookingQueryParametersDto query);
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int userId);
    Task<bool> UpdateBookingStatusAsync(int id, BookingStatus newStatus);
    Task<bool> CancelBookingAsync(int id, int currentUserId, bool isAdmin);
}