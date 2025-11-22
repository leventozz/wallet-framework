using WF.CustomerService.Domain.Enums;
using WF.CustomerService.Domain.ValueObjects;

namespace WF.CustomerService.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string IdentityId { get; private set; }
        public string CustomerNumber { get; private set; } = string.Empty;
        public PersonName Name { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public KycStatus KycStatus { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public bool IsDeleted { get; private set; }
        public string? DeletedBy { get; private set; }
        public bool IsActive { get; private set; }

        private Customer() { }

        public Customer(string identityId, PersonName name, Email email, string customerNumber, PhoneNumber phoneNumber)
        {
            Id = Guid.NewGuid();
            IdentityId = identityId;
            CustomerNumber = customerNumber;
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = null;
            IsDeleted = false;
            KycStatus = KycStatus.None;
            IsActive = true;
        }

        public void Update(PersonName? name = null, Email? email = null, PhoneNumber? phoneNumber = null)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot update a deleted customer.");

            if (name.HasValue)
                Name = name.Value;

            if (email.HasValue)
                Email = email.Value;

            if (phoneNumber.HasValue)
                PhoneNumber = phoneNumber.Value;

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
