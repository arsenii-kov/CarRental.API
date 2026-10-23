using System.ComponentModel.DataAnnotations;

namespace CarRental.API.DTOs
{
    public class CreateCarDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal DailyPrice { get; set; }
    }
}
