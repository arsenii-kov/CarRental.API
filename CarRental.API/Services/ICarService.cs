using CarRental.API.DTOs;

namespace CarRental.API.Services
{
    public interface ICarService
    {
        Task<IEnumerable<CarDto>> GetAllCarsAsync(CarQueryParametersDto query);
        Task<CarDto?> GetByIdAsync(int id);
        Task<IEnumerable<CarDto>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
        Task<CarDto> CreateCarAsync(CreateCarDto dto);
        Task<bool> UpdateCarAsync(int id, UpdateCarDto dto);
        Task<bool> DeleteCarAsync(int id);
    }
}
