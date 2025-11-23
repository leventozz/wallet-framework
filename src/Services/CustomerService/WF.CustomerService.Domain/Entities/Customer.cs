using WF.CustomerService.Domain.ValueObjects;
using WF.Shared.Contracts.Enums;
using WF.Shared.Contracts.Result;

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

        private Customer(string identityId, PersonName name, Email email, string customerNumber, PhoneNumber phoneNumber)
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
            KycStatus = KycStatus.Unverified;
            IsActive = true;
        }

        public static Result<Customer> Create(
            string identityId,
            PersonName name,
            Email email,
            string customerNumber,
            PhoneNumber phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(identityId))
                return Result<Customer>.Failure(Error.Validation("Customer.IdentityId.Required", "Identity ID cannot be null or empty."));

            if (string.IsNullOrWhiteSpace(customerNumber))
                return Result<Customer>.Failure(Error.Validation("Customer.CustomerNumber.Required", "Customer number cannot be null or empty."));

            var customer = new Customer(identityId, name, email, customerNumber, phoneNumber);
            return Result<Customer>.Success(customer);
        }

        public Result Update(PersonName? name = null, Email? email = null, PhoneNumber? phoneNumber = null)
        {
            if (IsDeleted)
                return Result.Failure(Error.Failure("Customer.Deleted", "Cannot update a deleted customer."));

            if (name.HasValue)
                Name = name.Value;

            if (email.HasValue)
                Email = email.Value;

            if (phoneNumber.HasValue)
                PhoneNumber = phoneNumber.Value;

            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result UpdateKycStatus(KycStatus status)
        {
            if (IsDeleted)
                return Result.Failure(Error.Failure("Customer.Deleted", "Cannot update KYC status of a deleted customer."));

            KycStatus = status;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result SetActive(bool isActive)
        {
            if (IsDeleted)
                return Result.Failure(Error.Failure("Customer.Deleted", "Cannot change active status of a deleted customer."));

            IsActive = isActive;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }

        public Result SoftDelete(string? deletedBy = null)
        {
            if (IsDeleted)
                return Result.Success();

            IsDeleted = true;
            IsActive = false;
            DeletedBy = deletedBy;
            UpdatedAtUtc = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
