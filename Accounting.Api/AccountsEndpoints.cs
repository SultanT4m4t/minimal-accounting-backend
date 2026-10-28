using Accounting.Api.Models;
using Accounting.Api.Services;
using Microsoft.AspNetCore.Mvc;

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
                group.MapGet("", GetAllAccounts);
                group.MapGet("{id:int}", GetAccountById).WithName(nameof(GetAccountById));
            }

            public void MapTransactionEndpoints()
            {
                var group = app.MapGroup("api/transactions");
                group.MapPost("", Transfer);
                group.MapGet("", GetAllTransactions);
                group.MapGet("{id:int}", GetTransactionById);
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
                return Results.Ok(new { fromBalance, toBalance });
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
