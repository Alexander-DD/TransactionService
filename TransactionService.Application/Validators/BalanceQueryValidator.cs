using FluentValidation;
using TransactionService.Application.UseCases.GetBalance;

namespace TransactionService.Application.Validators
{
    public class BalanceQueryValidator : AbstractValidator<BalanceQuery>
    {
        public BalanceQueryValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty()
                .WithMessage("Client ID is required.");
        }
    }
}