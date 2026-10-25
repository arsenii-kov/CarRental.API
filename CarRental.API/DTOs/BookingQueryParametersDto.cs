using CarRental.API.Models;

namespace CarRental.API.DTOs
{
    public class BookingQueryParametersDto
    {
        public BookingStatus? Status { get; set; }
        public int? CarId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomerName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
