using CarRental.API.DTOs;
using FluentValidation;

namespace CarRental.API.Validators
{
    public class UpdateBookingStatusDtoValidator : AbstractValidator<UpdateBookingStatusDto>
    {
        public UpdateBookingStatusDtoValidator()
        {
            RuleFor(x => x.Status).IsInEnum().WithMessage("Invalid booking status.");
        }
    }
}
