using System.ComponentModel.DataAnnotations;

namespace CarRental.API.DTOs
{
    public class CreateBookingDto
    {
        public int CarId { get; set; }
        public string CustomerName  { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public bool IncludeInsurance { get; set; }
    }
}
