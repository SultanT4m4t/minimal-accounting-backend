namespace Accounting.Api.Models
{
    public class Account
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public List<Transaction> Transactions = [];
    }
}
