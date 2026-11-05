using Accounting.Api.Models;
using Accounting.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Api
{
    public static class AccountsEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapAccountEndpoints()
            {
                var group = app.MapGroup("api/accounts");
                group.MapPost("", CreateAccount);
                group.MapGet("", GetAllAccounts).WithName(nameof(GetAllAccounts));
                group.MapGet("{id:int}", GetAccountById).WithName(nameof(GetAccountById));
                group.MapPost("/transfer", Transfer);
            }

            public void MapTransactionEndpoints()
            {
                var group = app.MapGroup("api/transactions");
                group.MapGet("", GetAllTransactions).WithName(nameof(GetAllTransactions));
                group.MapGet("/{id:int}", GetTransactionById).WithName(nameof(GetTransactionById));
            }
        }

        public static IResult CreateAccount(AccountStore store, [FromBody] CreateAccountDto dto)
        {
            try
            {
                var account = store.CreateAccount(dto.Name, dto.InitialBalance);
                return Results.CreatedAtRoute(nameof(GetAccountById), new { Id = account.Id }, account);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }

        public static IResult GetAllAccounts(AccountStore store)
        {
            return Results.Ok(store.GetAllAccounts());
        }

        public static IResult GetAccountById(AccountStore store, int id)
        {
            try
            {
                var account = store.GetAccountById(id);
                var accountDto = new AccountDto(account.Id, account.Name, account.Balance, account.Transactions);
                return Results.Ok(account);
            }
            catch (AccountNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        }

        public static IResult Transfer(AccountStore store, [FromBody] CreateTransferDto dto)
        {
            try
            {
                var (fromBalance, toBalance) = store.Transfer(dto.Amount, dto.FromAccountId, dto.ToAccountId);
                //Not providing a transaction Id because that will mean either returning a three-value tuple or a dto
                return Results.Ok(new { FromAccountBalance = fromBalance, ToAccountBalance = toBalance });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (AccountNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
            catch (InsufficientBalanceException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (DbUpdateException)
            {
                return Results.InternalServerError("An error occured during processing");
            }
        }

        public static IResult GetAllTransactions(AccountStore store) =>
            Results.Ok(store.GetAllTransactions());

        public static IResult GetTransactionById(AccountStore store, int id)
        {
            var transaction = store.GetTransactionById(id);
            return transaction is not null ?
                Results.Ok(transaction) :
                Results.NotFound($"No transaction found with {id}");
        }
    }
}
