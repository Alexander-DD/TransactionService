using FluentValidation;
using TransactionService.Application.UseCases.Revert;

namespace TransactionService.Application.Validators
{
    public class RevertCommandValidator : AbstractValidator<RevertCommand>
    {
        public RevertCommandValidator()
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("Transaction ID is required.");
        }
    }
}