namespace LovEat.API.DTOs
{
    public class SetupTwoFactorRequestDto { public int UserId { get; set; } }
    public class TwoFactorSetupResultDto { public string SecretForAuthenticatorApp { get; set; } = ""; public List<string> BackupCodes { get; set; } = new(); }
    public class VerifyTwoFactorRequestDto { public int UserId { get; set; } public string Code { get; set; } = ""; }
    public class TwoFactorStatusDto { public bool IsEnabled { get; set; } public DateTime? EnabledAt { get; set; } }
    public class DisableTwoFactorRequestDto { public int UserId { get; set; } public string Code { get; set; } = ""; }

    // Note: M162 Session Management DTOs already exist as UserSessionDto
    // in DeviceSecurityCorporateModels.cs — nothing new needed here.

    public class CreatePrivacyRequestDto { public int UserId { get; set; } public string RequestType { get; set; } = "Export"; }
    public class DataPrivacyRequestDto
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime RequestedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
    public class ProcessPrivacyRequestDto { public int RequestId { get; set; } public bool Approve { get; set; } = true; public string? RejectionReason { get; set; } }

    public class SecurityAuditEntryDto
    {
        public string EventType { get; set; } = "";
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
