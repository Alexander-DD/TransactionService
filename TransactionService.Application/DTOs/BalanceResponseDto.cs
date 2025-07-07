namespace TransactionService.Application.DTOs
{
    public class BalanceResponseDto
    {
        public DateTime BalanceDateTime { get; set; }
        public decimal ClientBalance { get; set; }
    }
}
