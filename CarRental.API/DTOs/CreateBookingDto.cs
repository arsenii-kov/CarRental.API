using System.ComponentModel.DataAnnotations;

namespace CarRental.API.DTOs
{
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "A vehicle ID is required!")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "The customer name is required!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "The customer name must be between 2 and 100 characters long!")]
        public string CustomerName  { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public bool IncludeInsurance { get; set; }
    }
}
