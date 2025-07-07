using System.ComponentModel.DataAnnotations;

namespace TransactionService.Infrastructure.Persistence.Entities
{
    public class TransactionEntity
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Amount { get; set; }
        public bool IsReverted => RevertTransactionId != null;
        public Guid? RevertTransactionId { get; set; }
        /// <summary>
        /// Need for optimistic blocking
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } 
    }
}
