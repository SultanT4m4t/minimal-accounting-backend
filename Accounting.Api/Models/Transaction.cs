namespace Accounting.Api.Models
{
    public class Transaction
    {
        public int Id { get; init; }
        public string Reference { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public decimal FromAccountBalance { get; init; }
        public decimal ToAccountBalance { get; init; }
        public bool Success { get; set; } = false;

        //Foreign Keys
        public int FromAccountId { get; init; }
        public int ToAccountId { get; init; }

        //Virtual Navigation Properties
        public Account FromAccount { get; set; }
        public Account ToAccount { get; set; }
    }
}
