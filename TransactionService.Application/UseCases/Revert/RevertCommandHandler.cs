using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Exceptions;

namespace TransactionService.Application.UseCases.Revert
{
    public class RevertCommandHandler : IRequestHandler<RevertCommand, RevertResponseDto>
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<RevertCommandHandler> _logger;

        public RevertCommandHandler(
            ITransactionRepository repository,
            ILogger<RevertCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RevertResponseDto> Handle(RevertCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing revert for transaction {request.TransactionId}");

            // Wrap operation in a transaction to ensure atomicity and avoid race conditions.
            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                var existing = await _repository.GetTransactionByIdAsync(request.TransactionId, cancellationToken);
                if (existing == null)
                {
                    _logger.LogWarning($"Transaction {request.TransactionId} not found for revert");
                    throw new DomainException("Transaction not found.");
                }

                // Ensure the idempotency of the operation "revert".
                if (existing.IsReverted)
                {
                    _logger.LogInformation($"Transaction {request.TransactionId} already reverted, returning existing revert");

                    if (existing.RevertTransactionId == null)
                    {
                        throw new DomainException("Transaction already reverted, but revert transaction ID is missing.");
                    }

                    var revertTransaction = await _repository.GetTransactionByIdAsync(existing.RevertTransactionId.Value, cancellationToken);
                
                    if (revertTransaction == null)
                    {
                        _logger.LogWarning($"Transaction {request.TransactionId} not found for revert");
                        throw new DomainException("RevertTransaction not found.");
                    }

                    var revertTransactionBalance = await _repository.GetClientBalanceAsync(revertTransaction.ClientId, revertTransaction.DateTime, cancellationToken);
                    return new RevertResponseDto
                    {
                        RevertDateTime = revertTransaction.DateTime,
                        ClientBalance = revertTransactionBalance
                    };
                }

                // Create revert transaction.
                var newRevertTransaction = new CreditTransaction
                {
                    Id = Guid.NewGuid(),
                    ClientId = existing.ClientId,
                    DateTime = DateTime.UtcNow,
                    Amount = -existing.Amount
                };

                await _repository.AddAsync(newRevertTransaction, cancellationToken);

                // Refresh existing transaction.
                await _repository.MarkTransactionAsRevertedAsync(existing.Id, newRevertTransaction.Id, cancellationToken);

                await _repository.SaveChangesAsync();

                _logger.LogInformation($"Successfully reverted transaction {request.TransactionId} with new transaction {newRevertTransaction.Id}");

                var balance = await _repository.GetClientBalanceAsync(existing.ClientId, cancellationToken);

                return new RevertResponseDto
                {
                    RevertDateTime = DateTime.UtcNow,
                    ClientBalance = balance
                };
            }, cancellationToken);
        }
    }
}
