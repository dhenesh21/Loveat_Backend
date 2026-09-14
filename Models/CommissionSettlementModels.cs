using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M41: Commission Management ─────────────────────────────────
    public class CommissionRule
    {
        [Key] public int Id { get; set; }
        public string RuleName        { get; set; } = "";
        public string AppliesTo       { get; set; } = "All";     // All / BookingType / City / Chef
        public string? TargetValue    { get; set; }
        public decimal PlatformPercent{ get; set; } = 15;        // default 15%
        public decimal ChefPercent    { get; set; } = 85;        // auto = 100 - Platform
        public bool   IsActive        { get; set; } = true;
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    }

    public class CommissionLedger
    {
        [Key] public int Id { get; set; }
        public int    BookingId         { get; set; }
        public int    ChefId            { get; set; }
        public User?  Chef              { get; set; }
        public decimal BookingAmount    { get; set; }
        public decimal PlatformAmount   { get; set; }
        public decimal ChefAmount       { get; set; }
        public decimal PlatformPercent  { get; set; }
        public int?   CommissionRuleId  { get; set; }
        public string Status            { get; set; } = "Pending"; // Pending / Settled / Held
        public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    }

    // ── M42: Chef Settlement & Payout ─────────────────────────────
    public class ChefBankAccount
    {
        [Key] public int Id { get; set; }
        public int    ChefId         { get; set; }
        public User?  Chef           { get; set; }
        public string AccountHolderName { get; set; } = "";
        public string AccountNumber  { get; set; } = "";  // masked in responses
        public string IFSC           { get; set; } = "";
        public string BankName       { get; set; } = "";
        public string AccountType    { get; set; } = "Savings"; // Savings / Current
        public bool   IsVerified     { get; set; } = false;
        public bool   IsPrimary      { get; set; } = true;
        public string? UpiId         { get; set; }
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    }

    public class SettlementRequest
    {
        [Key] public int Id { get; set; }
        public int    ChefId           { get; set; }
        public User?  Chef             { get; set; }
        public decimal Amount          { get; set; }
        public string Status           { get; set; } = "Pending"; // Pending / Processing / Completed / Failed / Rejected
        public string PaymentMethod    { get; set; } = "BankTransfer"; // BankTransfer / UPI
        public int?   BankAccountId    { get; set; }
        public ChefBankAccount? BankAccount { get; set; }
        public string? TransactionRef  { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime RequestedAt    { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt   { get; set; }
    }
}

namespace LovEat.API.DTOs
{
    // ── M41 DTOs ───────────────────────────────────────────────────
    public class CommissionRuleDto
    {
        public int     Id               { get; set; }
        public string  RuleName         { get; set; } = "";
        public string  AppliesTo        { get; set; } = "";
        public string? TargetValue      { get; set; }
        public decimal PlatformPercent  { get; set; }
        public decimal ChefPercent      { get; set; }
        public bool    IsActive         { get; set; }
    }

    public class CreateCommissionRuleDto
    {
        public string  RuleName         { get; set; } = "";
        public string  AppliesTo        { get; set; } = "All";
        public string? TargetValue      { get; set; }
        public decimal PlatformPercent  { get; set; } = 15;
    }

    public class CommissionSummaryDto
    {
        public decimal TotalBookingRevenue  { get; set; }
        public decimal TotalPlatformEarned  { get; set; }
        public decimal TotalChefPaid        { get; set; }
        public decimal PendingSettlement    { get; set; }
        public int     TotalTransactions    { get; set; }
        public List<CommissionLedgerDto> Recent { get; set; } = new();
        public List<CommissionRuleDto>   Rules  { get; set; } = new();
    }

    public class CommissionLedgerDto
    {
        public int     Id               { get; set; }
        public int     BookingId        { get; set; }
        public string  ChefName         { get; set; } = "";
        public decimal BookingAmount    { get; set; }
        public decimal PlatformAmount   { get; set; }
        public decimal ChefAmount       { get; set; }
        public decimal PlatformPercent  { get; set; }
        public string  Status           { get; set; } = "";
        public string  CreatedAt        { get; set; } = "";
    }

    // ── M42 DTOs ───────────────────────────────────────────────────
    public class ChefBankAccountDto
    {
        public int    Id                  { get; set; }
        public string AccountHolderName   { get; set; } = "";
        public string AccountNumberMasked { get; set; } = ""; // last 4 digits only
        public string IFSC                { get; set; } = "";
        public string BankName            { get; set; } = "";
        public string AccountType         { get; set; } = "";
        public bool   IsVerified          { get; set; }
        public bool   IsPrimary           { get; set; }
        public string? UpiId              { get; set; }
    }

    public class AddBankAccountDto
    {
        public string AccountHolderName { get; set; } = "";
        public string AccountNumber     { get; set; } = "";
        public string IFSC              { get; set; } = "";
        public string BankName          { get; set; } = "";
        public string AccountType       { get; set; } = "Savings";
        public string? UpiId            { get; set; }
    }

    public class WithdrawalRequestDto
    {
        public decimal Amount         { get; set; }
        public string  PaymentMethod  { get; set; } = "BankTransfer";
        public int?    BankAccountId  { get; set; }
    }

    public class SettlementRequestDto
    {
        public int     Id             { get; set; }
        public decimal Amount         { get; set; }
        public string  Status         { get; set; } = "";
        public string  PaymentMethod  { get; set; } = "";
        public string? TransactionRef { get; set; }
        public string? RejectionReason{ get; set; }
        public string  RequestedAt    { get; set; } = "";
        public string? ProcessedAt    { get; set; }
        public string  BankMasked     { get; set; } = "";
        // Admin list view needs to know WHO is asking to be paid — the
        // service already .Include()s Chef for this exact query, but this
        // DTO never surfaced it, so the admin settlement screen had no way
        // to show a requester name/phone at all.
        public string? ChefName       { get; set; }
        public string? Phone          { get; set; }
    }

    public class ChefWalletOverviewDto
    {
        public decimal AvailableBalance    { get; set; }
        public decimal TotalEarned         { get; set; }
        public decimal TotalWithdrawn      { get; set; }
        public decimal PendingSettlement   { get; set; }
        public decimal MinWithdrawal       { get; set; }
        public List<ChefBankAccountDto>    BankAccounts { get; set; } = new();
        public List<SettlementRequestDto>  History      { get; set; } = new();
    }
}
