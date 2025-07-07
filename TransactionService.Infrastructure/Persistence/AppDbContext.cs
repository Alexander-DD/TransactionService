using Microsoft.EntityFrameworkCore;
using TransactionService.Infrastructure.Persistence.Entities;

namespace TransactionService.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TransactionEntity> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionEntity>(entity =>
            {
                entity.ToTable("transactions");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ClientId).HasColumnName("client_id");
                entity.Property(e => e.DateTime).HasColumnName("datetime");
                entity.Property(e => e.Amount).HasColumnName("amount");
                entity.Property(e => e.RevertTransactionId).HasColumnName("revert_transaction_id");

                entity.Ignore(e => e.IsReverted);
            });
        }
    }
}
