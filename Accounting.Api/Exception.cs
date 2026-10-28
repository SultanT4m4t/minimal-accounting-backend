namespace Accounting.Api;

public class AccountNotFoundException(string message) : Exception(message);
public class InsufficientBalanceException(string message) : Exception(message);


