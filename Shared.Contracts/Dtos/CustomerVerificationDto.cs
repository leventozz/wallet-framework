using WF.Shared.Contracts.Enums;

namespace WF.Shared.Contracts.Dtos;

public record CustomerVerificationDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public KycStatus KycStatus { get; set; }
}

