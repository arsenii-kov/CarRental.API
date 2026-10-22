namespace CarRental.API.Models
{
        public enum BookingStatus
        {
            Pending = 1,  // Ожидает подтверждения
            Active = 2,  // Аренда активна (машина выдана)
            Completed = 3,  // Завершена успешно
            Cancelled = 4  // Отменена
        }
}
