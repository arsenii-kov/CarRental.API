namespace CarRental.API.DTOs
{
    public class CarDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal DailyPrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}
