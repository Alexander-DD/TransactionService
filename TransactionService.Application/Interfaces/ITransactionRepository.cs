using TransactionService.Domain.Interfaces;
namespace TransactionService.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task<ITransaction?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(ITransaction entity, CancellationToken cancellationToken);
        Task SaveChangesAsync();
        Task<decimal> GetClientBalanceAsync(Guid clientId, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the client's balance up to a certain time.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        Task<decimal> GetClientBalanceAsync(Guid clientId, DateTime dateTo, CancellationToken cancellationToken);

        /// <summary>
        /// Mark the old transaction as "isReverted" and add a link to a new transaction that will return the balance back.
        /// </summary>
        /// <param name="revertedTransactionId">Transaction to mark as "isReverted"</param>
        /// <param name="newRevertTransactionId">A new transaction that reverts an old transaction by reversing its changes</param>
        /// <returns></returns>
        Task MarkTransactionAsRevertedAsync(Guid revertedTransactionId, Guid newRevertTransactionId, CancellationToken cancellationToken);
        Task<List<ITransaction>> GetClientTransactionsAsync(Guid clientId, CancellationToken cancellationToken);
        //Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken);
    }
}
