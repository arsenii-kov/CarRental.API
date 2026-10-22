namespace CarRental.API.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string CustomerName  { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public int CarId { get; set; }
        public Car? Car { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }
}
