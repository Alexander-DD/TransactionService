namespace TransactionService.Application.DTOs
{
    public class RevertResponseDto
    {
        public DateTime RevertDateTime { get; set; }
        public decimal ClientBalance { get; set; }
    }
}
