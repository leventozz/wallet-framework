using WF.CustomerService.Domain.Enums;

namespace WF.CustomerService.Application.Dtos
{
    public record CustomerDto
    {
        public string CustomerNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public KycStatus KycStatus { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
