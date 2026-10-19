using System.ComponentModel.DataAnnotations;

namespace CarRental.API.DTOs
{
    public class CreateCarDto
    {
        [Required(ErrorMessage = "The brand name is required!")]
        [StringLength (50, MinimumLength = 2, ErrorMessage = "The brand name must be between 2 and 50 characters long!")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "The model name is required!")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "The model name must be between 1 and 50 characters long!")]
        public string Model { get; set; } = string.Empty;

        [Range  (1990, 2030, ErrorMessage = "The acceptable range is from 1990 to 2030.")]
        public int Year { get; set; }

        [Range(0.01, 100000.0, ErrorMessage = "The price must be greater than 0")]
        public decimal DailyPrice { get; set; }
    }
}
