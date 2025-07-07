using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;

namespace TransactionService.Application.UseCases.Credit
{
    public class CreditCommandHandler : IRequestHandler<CreditCommand, TransactionResponseDto>
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<CreditCommandHandler> _logger;

        public CreditCommandHandler(
            ITransactionRepository repository,
            ILogger<CreditCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> Handle(CreditCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing credit transaction {request.Id} for client {request.ClientId} with amount {request.Amount}");

            // Wrap operation in a transaction to ensure atomicity and avoid race conditions.
            return await _repository.ExecuteInTransactionAsync(async () =>
            {
                var existing = await _repository.GetTransactionByIdAsync(request.Id, cancellationToken);
                if (existing != null)
                {
                    _logger.LogInformation($"Transaction {request.Id} already exists, returning existing transaction");

                    var balance = await _repository.GetClientBalanceAsync(request.ClientId, existing.DateTime, cancellationToken);
                    return new TransactionResponseDto
                    {
                        InsertDateTime = existing.DateTime,
                        ClientBalance = balance
                    };
                }

                var creditTransaction = new CreditTransaction
                {
                    Id = request.Id,
                    ClientId = request.ClientId,
                    DateTime = request.DateTime,
                    Amount = request.Amount
                };

                await _repository.AddAsync(creditTransaction, cancellationToken);
                await _repository.SaveChangesAsync();

                _logger.LogInformation($"Credit transaction {request.Id} successfully created for client {request.ClientId}");

                var updatedBalance = await _repository.GetClientBalanceAsync(request.ClientId, cancellationToken);

                return new TransactionResponseDto
                {
                    InsertDateTime = creditTransaction.DateTime,
                    ClientBalance = updatedBalance
                };
            }, cancellationToken);
        }
    }
}
