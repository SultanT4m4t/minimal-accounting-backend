using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

Ussd ussd = new();
//await ussd.SeedDataAsync();
Console.WriteLine("Welcome to MobileBank");
try
{
    do
    {
        Console.Write("Enter account number: ");
        var accountString = Console.ReadLine();
        var isValidInt = int.TryParse(accountString, out var accountId);
        if (!isValidInt)
        {
            Console.WriteLine("Invalid option. Try again");
            continue;
        }
        var account = await ussd.GetAccountIdAsync(accountId);
        if (account is null)
        {
            Console.WriteLine("Account not found");
            Console.Read();
            break;
        }

        Console.WriteLine($"Account {account}");
        Console.WriteLine("1. Check balance\n2. Transfer funds\n3. Account statement\n-----------\n");
        Console.Write("Enter option: ");
        var input = Console.ReadLine();
        //Route to option number
        switch (input)
        {

            case "1":
                while (true)
                {
                    Console.WriteLine(await ussd.CheckBalanceAsync());
                    break;
                }
                //This break is for the switch statement
                break;

            case "2":
                while (true)
                {
                    //One wrong entry for amount or account number restarts both. Don't want to nest it in more while loops
                    Console.Write("Enter recipient's account number: ");
                    var toAccountString = Console.ReadLine();
                    var isInt = int.TryParse(toAccountString, out var toAccountId);
                    if (!isInt)
                    {
                        Console.WriteLine("Invalid option. Try again");
                        continue;
                    }

                    Console.Write("Enter amount: ");
                    var amountString = Console.ReadLine();
                    var isDecimal = int.TryParse(toAccountString, out var amount);
                    if (!isDecimal)
                    {
                        Console.WriteLine("Invalid option. Try again");
                        continue;
                    }

                    var transfer = await ussd.TransferFundsAsync(amount, toAccountId);
                    Console.WriteLine(transfer);
                    break;
                }
                //This break is for the switch statement
                break;

            case "3":
                Console.WriteLine(await ussd.GetTransactionsAsync(accountId));
                break;

            default:
                Console.Write("Invalid input. Try again\n");
                continue;
        }
        Console.WriteLine("Press any key to reload menu or Escape to exit");
    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.Read();
}

internal class Ussd
{
    private readonly HttpClient _client = new(new SocketsHttpHandler());
    private const string BaseUri = "http://localhost:5079/api/accounts";
    private int _account;

    public async Task<int?> GetAccountIdAsync(int accountId)
    {
        var accountResponse = await _client.GetAsync($"{BaseUri}/{accountId}");
        if (!accountResponse.IsSuccessStatusCode)
        {
            return accountResponse.StatusCode == HttpStatusCode.NotFound
                ? null
                : throw new HttpRequestException("An error occured during request. Kindly retry again");
        }
        _account = accountId;
        return _account;
    }

    public async Task<decimal> CheckBalanceAsync()
    {
        using var doc = JsonDocument.Parse(await _client.GetStringAsync($"{BaseUri}/{_account}"));
        var balanceString = doc.RootElement.GetProperty("balance").GetDecimal();
        return balanceString;
    }

    public async Task<string> GetTransactionsAsync(int accountId)
    {
        var response = await _client.GetAsync($"{BaseUri}/{accountId}");
        if (response.StatusCode == HttpStatusCode.NotFound) return "Account not found";
        if (response.IsSuccessStatusCode)
        {
            var element = await response.Content.ReadFromJsonAsync<JsonElement>();
            var transactions = element.GetProperty("transactions").EnumerateArray();
            StringBuilder sb = new();
            int i = 1;
            foreach (var transaction in transactions)
            {
                var id = transaction.GetProperty("id").GetInt32();
                var fromAccount = transaction.GetProperty("fromAccountId").GetInt32();
                var toAccount = transaction.GetProperty("toAccountId").GetInt32();
                var date = transaction.GetProperty("createdAt").GetDateTime();
                var isSuccess = transaction.GetProperty("success").GetBoolean();
                //For when the account is the sender or receiver
                sb.Append(_account == fromAccount
                    ? $"{i++}. Transfer out -- {date:dd/MM/yyyy} | Transaction ID: {id} | To: {toAccount}\n"
                    : $"{i++}. Transfer in -- {date:dd/MM/yyyy} | Transaction ID: {id} | From: {fromAccount}\n");
            }

            return !string.IsNullOrWhiteSpace(sb.ToString()) ? sb.ToString() : "No transactions on account";
        }

        throw new HttpRequestException("An error occurred during the request");
    }

    public async Task<string> TransferFundsAsync(decimal amount, int toAccountId)
    {
        var transferResponse = await _client.PostAsJsonAsync($"{BaseUri}/transfer", new { Amount = amount, FromAccountId = _account, ToAccountId = toAccountId });

        if (transferResponse.IsSuccessStatusCode) return $"Successfully transferred an amount of {amount} to Account {toAccountId}";

        if (transferResponse.StatusCode == HttpStatusCode.NotFound) return "Recipient account not found";

        return (int)transferResponse.StatusCode == 400
                    ? await transferResponse.Content.ReadAsStringAsync()
                    : "An error occured while processing your request";
    }

    public async Task SeedDataAsync()
    {
        var transactionUri = "http://localhost:5079/api/accounts/transfer";
        var cash = new { Name = "Cash", InitialBalance = 900 };
        var expenses = new { Name = "Expenses", InitialBalance = 900 };
        var transfer = new { Amount = 800, FromAccountId = 1, ToAccountId = 2 };

        using var cashResponse = await _client.PostAsJsonAsync(BaseUri, cash);
        using var expenseResponse = await _client.PostAsJsonAsync(BaseUri, expenses);

        var transferResponse = await _client.PostAsJsonAsync(transactionUri, transfer);
    }
}