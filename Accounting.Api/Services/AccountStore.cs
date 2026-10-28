using Accounting.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Api.Services;

public class AccountStore(IServiceScopeFactory scopeFactory)
{
    private readonly Lock _transferLock = new();
    private static int _accountCounter = 0;
    private static int _transactionCounter = 0;

    public Account CreateAccount(string name, decimal initialBalance = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(initialBalance, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var account = new Account
        {
            Id = AssignAccountId(),
            Name = name,
            Balance = initialBalance
        };

        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Accounts.Add(account);
        context.SaveChanges();

        return account;
    }

    public List<Account> GetAllAccounts()
    {
        var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return context.Accounts.ToList();
    }



    public Account GetAccountById(int id)
    {
        var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = context.Accounts.FirstOrDefault(a => a.Id == id && a.IsActive == true);
        return account ?? throw new AccountNotFoundException($"Account with Id:{id} does not exist");
    }

    public (decimal fromBalance, decimal toBalance) Transfer(decimal amount, int fromAccountId, int toAccountId, string reference = "")
    {

        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0);

        //Cannot use lock object with async/await. Consider using SemaphoreSlim for that
        //Async/Await makes no difference here since it is an in-memory database
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        lock (_transferLock)
        {
            if (fromAccountId == toAccountId)
                throw new InvalidOperationException("Sender and Receiver account cannot be the same");

            var fromAccount = context.Accounts.FirstOrDefault(a => a.Id == fromAccountId && a.IsActive == true) ?? throw new AccountNotFoundException("Invalid sender account details");

            if (amount > fromAccount.Balance) throw new InsufficientBalanceException("Insufficient balance");

            var toAccount = context.Accounts.FirstOrDefault(a => a.Id == toAccountId && a.IsActive == true) ?? throw new AccountNotFoundException("Invalid receiver account details");

            try
            {
                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                var transaction = new Transaction
                {
                    Id = AssignTransactionId(),
                    FromAccountId = fromAccountId,
                    ToAccountId = toAccountId,
                    Reference = reference,
                    FromAccountBalance = fromAccount.Balance,
                    ToAccountBalance = toAccount.Balance
                };

                //fromAccount.Transactions.Add(transaction);
                context.SaveChanges();

                context.Transactions.Add(transaction);
                //Assuming there is no error during save here
                context.SaveChanges();
                return (fromAccount.Balance, toAccount.Balance);
            }
            catch (DbUpdateException)
            {
                fromAccount.Balance += amount;
                toAccount.Balance -= amount;
                throw;
            }
        }
    }

    public List<Transaction> GetAllTransactions()
    {
        var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return context.Transactions.ToList();
    }

    public Transaction? GetTransactionById(int id)
    {
        var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return context.Transactions.FirstOrDefault(t => t.Id == id);
    }

    private static int AssignAccountId()
    {
        return Interlocked.Increment(ref _accountCounter);
    }

    private static int AssignTransactionId()
    {
        return Interlocked.Increment(ref _transactionCounter);
    }

    //private static decimal CheckAmount(decimal amount) =>
    //    amount > 0 ? amount : throw new ArgumentOutOfRangeException(paramName: nameof(amount), message: "Amount cannot be negative");
}

