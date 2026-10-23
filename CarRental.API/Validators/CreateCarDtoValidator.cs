using CarRental.API.DTOs;
using FluentValidation;


namespace CarRental.API.Validators
{
    public class CreateCarDtoValidator : AbstractValidator<CreateCarDto>
    {
        public CreateCarDtoValidator()
        {
            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .Length(2, 50).WithMessage("The brand name must be between 2 and 50 characters long.");
            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required.")
                .MaximumLength(50);
            RuleFor(x => x.DailyPrice)
            .GreaterThan(0).WithMessage("Daily price must be greater than 0.");
            RuleFor(x => x.Year)
            .InclusiveBetween(1990, DateTime.UtcNow.Year + 1)
            .WithMessage("Year must be valid.");

        }
    }
}
