namespace CarRental.API.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IncludeInsurance { get; set; }
    }
}

