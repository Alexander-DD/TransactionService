namespace TransactionService.Domain.Interfaces
{
    public interface ITransaction
    {
        Guid Id { get; }
        Guid ClientId { get; }
        DateTime DateTime { get; }
        decimal Amount { get; }
        bool IsReverted { get; }
        Guid? RevertTransactionId { get; }
    }
}
