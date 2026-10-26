using CarRental.API.DTOs;
using FluentValidation;


namespace CarRental.API.Validators
{
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {

            RuleFor(x => x.CarId).GreaterThan(0).WithMessage("Car ID must be greater than 0.");

            RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow);

            RuleFor(x => x.EndTime).GreaterThan(x => x.StartDate).WithMessage("End date must be later than start date.");
        }
    }
}
