namespace WF.TransactionService.Domain.States
{
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        FraudCheckApproved,
        FraudCheckDeclined,
        SenderDebited,
        ReceiverCredited
    }
}
