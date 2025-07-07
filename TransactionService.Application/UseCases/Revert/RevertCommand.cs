using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.UseCases.Revert
{
    public class RevertCommand : IRequest<RevertResponseDto>
    {
        public Guid TransactionId { get; init; }
    }
}
