using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Exceptions;

namespace TransactionService.Application.UseCases.Debit
{
    public class DebitCommandHandler : IRequestHandler<DebitCommand, TransactionResponseDto>
    {
        private readonly ITransactionRepository _repository;
        private readonly ILogger<DebitCommandHandler> _logger;

        public DebitCommandHandler(
            ITransactionRepository repository,
            ILogger<DebitCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TransactionResponseDto> Handle(DebitCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing debit transaction {request.Id} for client {request.ClientId} with amount {request.Amount}");
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

                var currentBalance = await _repository.GetClientBalanceAsync(request.ClientId, cancellationToken);
                if (currentBalance < request.Amount)
                {
                    _logger.LogWarning($"Insufficient funds for client {request.ClientId}. Balance: {currentBalance}, Requested: {request.Amount}");
                    throw new DomainException("Insufficient funds.");
                }

                var debitTransaction = new DebitTransaction
                {
                    Id = request.Id,
                    ClientId = request.ClientId,
                    DateTime = request.DateTime,
                    Amount = -request.Amount
                };

                await _repository.AddAsync(debitTransaction, cancellationToken);
                await _repository.SaveChangesAsync();

                _logger.LogInformation($"Debit transaction {request.Id} successfully created for client {request.ClientId}");

                var updatedBalance = await _repository.GetClientBalanceAsync(request.ClientId, cancellationToken);

                return new TransactionResponseDto
                {
                    InsertDateTime = debitTransaction.DateTime,
                    ClientBalance = updatedBalance
                };
            }, cancellationToken);
        }
    }
}
