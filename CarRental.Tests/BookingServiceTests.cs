using CarRental.API.Data;
using CarRental.API.DTOs;
using CarRental.API.Models;
using CarRental.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarRental.Tests;

public class BookingServiceTests
{
    // Фабричный метод: для каждого теста создаёт уникальную базу в памяти
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    #region 1. Проверка доступности авто и валидации

    [Fact]
    public async Task CreateBookingAsync_CarDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var service = new BookingService(context);
        var dto = new CreateBookingDto
        {
            CarId = 999, // машины с таким Id нет
            StartDate = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(3),
            IncludeInsurance = false
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_CarIsNotAvailable_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car
        {
            Id = 1,
            Brand = "Toyota",
            Model = "Camry",
            DailyPrice = 100m,
            IsAvailable = false // машина на обслуживании
        };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(3)
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region 2. Проверка конфликтов дат (Overlapping)

    [Fact]
    public async Task CreateBookingAsync_DatesOverlapWithExistingBooking_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "BMW", Model = "320i", DailyPrice = 100m, IsAvailable = true };
        context.Cars.Add(car);

        var baseDate = DateTime.UtcNow.Date.AddDays(5);

        // Существующая бронь: с 5 по 10 число
        context.Bookings.Add(new Booking
        {
            CarId = 1,
            UserId = 2,
            StartDate = baseDate,
            EndTime = baseDate.AddDays(5),
            Status = BookingStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Новая бронь внутри существующей: с 6 по 8 число
        var overlappingDto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = baseDate.AddDays(1),
            EndTime = baseDate.AddDays(3)
        };

        // Act
        var result = await service.CreateBookingAsync(overlappingDto, userId: 1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_ExistingBookingIsCancelled_AllowsNewBooking()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "BMW", Model = "320i", DailyPrice = 100m, IsAvailable = true };
        context.Cars.Add(car);

        var baseDate = DateTime.UtcNow.Date.AddDays(5);

        // Бронь на эти же даты, но она отменена (Cancelled)
        context.Bookings.Add(new Booking
        {
            CarId = 1,
            UserId = 2,
            StartDate = baseDate,
            EndTime = baseDate.AddDays(5),
            Status = BookingStatus.Cancelled
        });
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = baseDate,
            EndTime = baseDate.AddDays(5),
            IncludeInsurance = false
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        result.Should().NotBeNull();
        result!.CarId.Should().Be(1);
    }

    #endregion

    #region 3. Расчёт цены, скидок и страховки

    [Fact]
    public async Task CreateBookingAsync_Standard3DaysWithoutInsurance_CalculatesCorrectPrice()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "Skoda", Model = "Octavia", DailyPrice = 100m, IsAvailable = true };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(3); // 3 дня -> скидок нет (нужно >= 4 дней)

        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = startDate,
            EndTime = endDate,
            IncludeInsurance = false
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        // 3 дня * 100 = 300
        result.Should().NotBeNull();
        result!.TotalPrice.Should().Be(300m);
    }

    [Fact]
    public async Task CreateBookingAsync_Between4And9Days_Applies5PercentDiscount()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "Skoda", Model = "Octavia", DailyPrice = 100m, IsAvailable = true };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(5); // 5 дней -> скидка 5%

        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = startDate,
            EndTime = endDate,
            IncludeInsurance = false
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        // Базовая цена: 5 * 100 = 500
        // Скидка 5%: 500 * 0.95 = 475
        result.Should().NotBeNull();
        result!.TotalPrice.Should().Be(475m);
    }

    [Fact]
    public async Task CreateBookingAsync_10DaysOrMore_Applies10PercentDiscount()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "Audi", Model = "A4", DailyPrice = 200m, IsAvailable = true };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(10); // 10 дней -> скидка 10%

        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = startDate,
            EndTime = endDate,
            IncludeInsurance = false
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        // Базовая цена: 10 * 200 = 2000
        // Скидка 10%: 2000 * 0.90 = 1800
        result.Should().NotBeNull();
        result!.TotalPrice.Should().Be(1800m);
    }

    [Fact]
    public async Task CreateBookingAsync_WithInsurance_Adds50PerDay()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var car = new Car { Id = 1, Brand = "Ford", Model = "Focus", DailyPrice = 100m, IsAvailable = true };
        context.Cars.Add(car);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var endDate = startDate.AddDays(2); // 2 дня, без скидок

        var dto = new CreateBookingDto
        {
            CarId = 1,
            StartDate = startDate,
            EndTime = endDate,
            IncludeInsurance = true // +50 в день
        };

        // Act
        var result = await service.CreateBookingAsync(dto, userId: 1);

        // Assert
        // Аренда: 2 * 100 = 200
        // Страховка: 2 * 50 = 100
        // Итого: 300
        result.Should().NotBeNull();
        result!.TotalPrice.Should().Be(300m);
    }

    #endregion

    #region 4. Отмена бронирования (CancelBookingAsync)

    [Fact]
    public async Task CancelBookingAsync_UserTriesToCancelOtherUsersBooking_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var booking = new Booking
        {
            Id = 1,
            CarId = 1,
            UserId = 10, // владелец — юзер 10
            StartDate = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(7),
            Status = BookingStatus.Pending
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: пытается отменить юзер 99 (не админ)
        var result = await service.CancelBookingAsync(id: 1, currentUserId: 99, isAdmin: false);

        // Assert
        result.Should().BeFalse();

        // Убеждаемся, что статус в базе не поменялся
        var bookingInDb = await context.Bookings.FindAsync(1);
        bookingInDb!.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task CancelBookingAsync_AdminCanCancelAnyBooking_ReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var booking = new Booking
        {
            Id = 1,
            CarId = 1,
            UserId = 10,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndTime = DateTime.UtcNow.AddDays(7),
            Status = BookingStatus.Pending
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act: отменяет админ (userId не совпадает, но isAdmin == true)
        var result = await service.CancelBookingAsync(id: 1, currentUserId: 999, isAdmin: true);

        // Assert
        result.Should().BeTrue();

        var bookingInDb = await context.Bookings.FindAsync(1);
        bookingInDb!.Status.Should().Be(BookingStatus.Cancelled);
    }

    #endregion
}

