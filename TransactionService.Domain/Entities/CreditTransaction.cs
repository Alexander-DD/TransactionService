using TransactionService.Domain.Interfaces;

namespace TransactionService.Domain.Entities
{
    public class CreditTransaction : ITransaction
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public DateTime DateTime { get; init; }
        public decimal Amount { get; init; }
        public bool IsReverted { get; init; }
        public Guid? RevertTransactionId { get; init; }
    }
}
