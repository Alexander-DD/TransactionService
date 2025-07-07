using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Entities;

namespace TransactionService.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _dbContext;

        public TransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ITransaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Transactions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null) return null;

            return MapToDomain(entity);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task AddAsync(ITransaction transaction, CancellationToken cancellationToken)
        {
            var entity = new TransactionEntity
            {
                Id = transaction.Id,
                ClientId = transaction.ClientId,
                DateTime = transaction.DateTime,
                Amount = transaction.Amount
            };

            await _dbContext.Transactions.AddAsync(entity, cancellationToken);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<decimal> GetClientBalanceAsync(Guid clientId, CancellationToken cancellationToken)
        {
            var sum = await _dbContext.Transactions
                .Where(x => x.ClientId == clientId)
                .SumAsync(x => x.Amount, cancellationToken);

            return sum;
        }

        public async Task<decimal> GetClientBalanceAsync(Guid clientId, DateTime dateTo, CancellationToken cancellationToken)
        {
            var sum = await _dbContext.Transactions
                .Where(x => x.ClientId == clientId 
                    && x.DateTime <= dateTo)
                .SumAsync(x => x.Amount, cancellationToken);

            return sum;
        }

        public async Task<List<ITransaction>> GetClientTransactionsAsync(Guid clientId, CancellationToken cancellationToken)
        {
            var list = await _dbContext.Transactions
                .Where(x => x.ClientId == clientId)
                .ToListAsync(cancellationToken);

            return list.Select(MapToDomain).ToList();
        }

        private ITransaction MapToDomain(TransactionEntity entity)
        {
            if (entity.Amount >= 0)
            {
                return new CreditTransaction
                {
                    Id = entity.Id,
                    ClientId = entity.ClientId,
                    DateTime = entity.DateTime,
                    Amount = entity.Amount,
                    IsReverted = entity.IsReverted,
                    RevertTransactionId = entity.RevertTransactionId
                };
            }
            else
            {
                return new DebitTransaction
                {
                    Id = entity.Id,
                    ClientId = entity.ClientId,
                    DateTime = entity.DateTime,
                    Amount = entity.Amount,
                    IsReverted = entity.IsReverted,
                    RevertTransactionId = entity.RevertTransactionId
                };
            }
        }

        public async Task MarkTransactionAsRevertedAsync(Guid revertedTransactionId, Guid newRevertTransactionId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Transactions
                .FirstOrDefaultAsync(x => x.Id == revertedTransactionId, cancellationToken);

            if (entity != null)
            {
                entity.RevertTransactionId = newRevertTransactionId;
            }
        }
    }
}
