using WF.CustomerService.Domain.Enums;

namespace WF.CustomerService.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string CustomerNumber { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public KycStatus KycStatus { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public bool IsDeleted { get; private set; }
        public string? DeletedBy { get; private set; }
        public bool IsActive { get; private set; }

        private Customer() { }

        public Customer(string firstName, string lastName, string email, string customerNumber, string phoneNumber)
        {
            Id = Guid.NewGuid();
            CustomerNumber = customerNumber;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            IsDeleted = false;
            KycStatus = KycStatus.None;
            IsActive = true;
        }

        public void Update(string? firstName = null, string? lastName = null, string? email = null, string? phoneNumber = null)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update a deleted customer.");

            if (!string.IsNullOrWhiteSpace(firstName))
                FirstName = firstName;

            if (!string.IsNullOrWhiteSpace(lastName))
                LastName = lastName;

            if (!string.IsNullOrWhiteSpace(email))
                Email = email;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                PhoneNumber = phoneNumber;

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void UpdateKycStatus(KycStatus status)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update KYC status of a deleted customer.");

            KycStatus = status;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot change active status of a deleted customer.");

            IsActive = isActive;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void SoftDelete(string? deletedBy = null)
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
            IsActive = false;
            DeletedBy = deletedBy;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
