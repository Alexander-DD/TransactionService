using FluentValidation;
using TransactionService.Application.UseCases.Debit;

namespace TransactionService.Application.Validators
{
    public class DebitCommandValidator : AbstractValidator<DebitCommand>
    {
        public DebitCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be positive.");

            RuleFor(x => x.DateTime)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Date cannot be in the future.");

            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ClientId).NotEmpty();
        }
    }
}
