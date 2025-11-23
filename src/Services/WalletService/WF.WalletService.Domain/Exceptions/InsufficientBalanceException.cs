namespace WF.WalletService.Domain.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(decimal balance, decimal requestedAmount)
            : base($"Insufficient balance. Current balance: {balance}, Requested amount: {requestedAmount}")
        {
            Balance = balance;
            RequestedAmount = requestedAmount;
        }

        public InsufficientBalanceException(decimal requestedAmount)
            : base($"Insufficient balance. Requested amount: {requestedAmount}")
        {
            Balance = 0;
            RequestedAmount = requestedAmount;
        }

        public decimal Balance { get; }
        public decimal RequestedAmount { get; }
    }
}
