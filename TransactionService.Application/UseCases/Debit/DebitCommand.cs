using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.UseCases.Debit
{
    public class DebitCommand : IRequest<TransactionResponseDto>
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public DateTime DateTime { get; init; }
        public decimal Amount { get; init; }
    }
}
