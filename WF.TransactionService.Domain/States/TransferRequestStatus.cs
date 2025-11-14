namespace WF.TransactionService.Domain.States
{
    public enum TransferRequestStatus
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
