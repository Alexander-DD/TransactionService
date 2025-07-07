using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.UseCases.GetBalance
{
    public class BalanceQueryHandler : IRequestHandler<BalanceQuery, BalanceResponseDto>
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<BalanceQueryHandler> _logger;

        public BalanceQueryHandler(
            ITransactionRepository repository,
            ILogger<BalanceQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BalanceResponseDto> Handle(BalanceQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing balance query for client {request.ClientId}");

            var balanceDateTime = DateTime.UtcNow;
            var balance = await _repository.GetClientBalanceAsync(request.ClientId, cancellationToken);

            _logger.LogInformation($"Balance query completed for client {request.ClientId}. Balance: {balance}");

            return new BalanceResponseDto
            {
                BalanceDateTime = balanceDateTime,
                ClientBalance = balance
            };
        }
    }
}
