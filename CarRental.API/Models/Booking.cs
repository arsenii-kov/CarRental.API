namespace CarRental.API.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public int CarId { get; set; }
        public Car? Car { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public bool IncludeInsurance { get; set; }

    }
}
