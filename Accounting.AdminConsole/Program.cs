using Accounting.AdminConsole;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;

const string menu = "\t1. Get all accounts.\n\t2. Get Account by Id\n\t3. Create Account\n\t4. Initiate Transfer\n\t5. Exit (Or press Q)";
AdminConsole console = new();

Console.WriteLine("Welcome to Accountant Admin Console");
try
{
    //Local machine specific addition. For some reason, when both Console and API projects are started, Console finishes ahead and SeedDataAsync meets a "No connection" error
    await Task.Delay(500);
    await console.SeedDataAsync();
    do
    {
        Console.WriteLine($"Accounting Admin Dashboard:\n{menu}");
        Console.Write("\nEnter option number: ");
        var input = Console.ReadLine();
        if (input == "1")
        {
            var accounts = await console.GetAllAccounts();
            foreach (var account in accounts)
            {
                Console.WriteLine(account);
            }
        }
        else if (input == "2")
        {
            Console.Write("Enter account id: ");
            var isInt = int.TryParse(Console.ReadLine(), out int accountId);
            if (isInt)
            {
                var account = await console.GetAccountById(accountId);
                Console.WriteLine(account is not null ? $"{account}" : $"Account with Id: {accountId} does not exist");
            }
            else
            {
                Console.WriteLine("Account Id can only contain numbers");
            }
        }
        else if (input == "3")
        {
            Console.Write("Enter a name for the account: ");
            var name = Console.ReadLine();
            Console.Write("Enter initial balance: ");
            var isDecimal = decimal.TryParse(Console.ReadLine(), out decimal initialBalance);
            var account = isDecimal && initialBalance > 0
                ? await console.CreateAccount(name!, initialBalance)
                : await console.CreateAccount(name!);
            Console.WriteLine(isDecimal && initialBalance > 0
                ? $"{account}"
                : $"Initial balance can only be a positive whole number or decimal. Default balance will be set to 0.\n {account}");
        }

        else if (input == "4")
        {
            Console.Write("Enter source account number: ");
            var isInt = int.TryParse(Console.ReadLine(), out int fromId);

            Console.Write("Enter destination account number: ");
            var isInt1 = int.TryParse(Console.ReadLine(), out int toId);

            if (!isInt || !isInt1)
            {
                Console.WriteLine("Account Ids can only be integers\nPress any key to return to menu");
                continue;
            }
            Console.Write("Enter amount to transfer: ");
            var isDecimal = decimal.TryParse(Console.ReadLine(), out decimal amount);

            if (!isDecimal)
            {
                Console.WriteLine("Please enter a valid amount");
            }
            var account = await console.InitiateTransfer(amount, fromId, toId);
            Console.WriteLine(account);

        }

        else if ("q".Equals(input, StringComparison.InvariantCultureIgnoreCase) || input == "5")
        {
            break;
        }

        else
        {
            Console.WriteLine("Invalid option...");
        }

        Console.WriteLine("Press Esc to quit or any key to return to menu)\n");

    } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

    Console.WriteLine("Exiting...");
    if (Debugger.IsAttached) Console.Read();
}


catch (HttpRequestException ex) when (ex.InnerException is SocketException)
{
    Console.WriteLine($"Status Code: 504\nAn error occured during the request. See details below\n{ex.Message}");
    Console.Read();
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"An error occured while processing request. See below for details\n{ex.Message}");
    Console.Read();
}
catch (JsonException ex)
{
    Console.WriteLine(ex.Message);
    Console.Read();
}