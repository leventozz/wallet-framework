namespace WF.CustomerService.Domain.Enums
{
    public enum KycStatus
    {
        None = 0,       // KYC süreci hiç başlamamış
        Pending = 1,    // İncelemede
        Approved = 2,   // Onaylanmış
        Rejected = 3    // Reddedilmiş
    }
}
