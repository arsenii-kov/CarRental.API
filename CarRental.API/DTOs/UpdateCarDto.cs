using System.ComponentModel.DataAnnotations;

namespace CarRental.API.DTOs
{
    public class UpdateCarDto
    {
        [Range(0.01, 100000.0, ErrorMessage = "The price must be greater than 0")]
        public decimal DailyPrice { get; set; }

        public bool IsAvailable { get; set; } 
    }
}
