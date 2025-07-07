using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.UseCases.GetBalance
{
    public class BalanceQuery : IRequest<BalanceResponseDto>
    {
        public Guid ClientId { get; init; }
    }
}
