using System.Net;
using System.Net.Http.Json;
using System.Text.Json;




namespace Accounting.AdminConsole
{
    internal class Transaction
    {
        public int Id { get; init; }
        public string? Reference { get; init; }
        public DateTime CreatedAt { get; init; }
        public decimal FromAccountBalance { get; init; }
        public decimal ToAccountBalance { get; init; }
        public bool Success { get; set; } = false;
        public int FromAccountId { get; init; }
        public int ToAccountId { get; init; }
    }
    internal class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<Transaction> Transactions { get; set; }

        public override string ToString()
        {
            return $"Account: Id: {Id} Name: {Name} Balance: {Balance} CreatedAt: {CreatedAt} IsActive {IsActive}";
        }
    }

    internal class AdminConsole
    {
        private const string BaseUri = "http://localhost:5079/api/accounts";
        private readonly HttpClient _client = new(new SocketsHttpHandler());

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        internal async Task SeedDataAsync()
        {
            var cash = new { Name = "Cash", InitialBalance = 900 };
            var expenses = new { Name = "Expenses", InitialBalance = 900 };

            using var cashResponse = await _client.PostAsJsonAsync(BaseUri, cash);
            using var expenseResponse = await _client.PostAsJsonAsync(BaseUri, expenses);

            Console.WriteLine(cashResponse.IsSuccessStatusCode
                ? "\"Cash\" Account created successfully"
                : await expenseResponse.Content.ReadAsStringAsync());

            Console.WriteLine(expenseResponse.IsSuccessStatusCode
                ? "\"Expenses\" Account created successfully"
                : await expenseResponse.Content.ReadAsStringAsync());
        }

        internal async Task<Account?> CreateAccount(string name, decimal initialBalance = 0)
        {
            var response = await _client.PostAsJsonAsync(BaseUri, new { Name = name, InitialBalance = initialBalance });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Account>(_options);
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed: Status Code {(int)response.StatusCode} - {error}");
        }

        internal async Task<List<Account>> GetAllAccounts()
        {
            var response = await _client.GetAsync(BaseUri);
            var debug = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"{(int)response.StatusCode} -- {await response.Content.ReadAsStringAsync()}");
            }

            var accounts = await response.Content.ReadFromJsonAsync<List<Account>>(_options) ?? [];
            return accounts;
        }

        internal async Task<Account?> GetAccountById(int id)
        {
            var response = await _client.GetAsync($"{BaseUri}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Account>(_options);
            }

            return response.StatusCode == HttpStatusCode.NotFound
                ? null
                : throw new HttpRequestException(
                    $"{(int)response.StatusCode} -- {await response.Content.ReadAsStringAsync()}");
        }
    }
}