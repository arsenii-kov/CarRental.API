using FluentValidation;
using CarRental.API.DTOs;

namespace CarRental.API.Validators;

public class UpdateCarDtoValidator : AbstractValidator<UpdateCarDto>
{
    public UpdateCarDtoValidator()
    {
        RuleFor(x => x.DailyPrice)
            .GreaterThan(0)
            .WithMessage("The price must be greater than 0");
    }
}