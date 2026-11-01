namespace Accounting.Api.Models
{
    public record CreateAccountDto(string Name, decimal InitialBalance = 0);

    public record AccountDto(int Id, string Name, decimal Balance, List<Transaction> Transactions);

    public record UpdateAccountDto();

    public record CreateTransferDto(decimal Amount, int FromAccountId, int ToAccountId);
}
