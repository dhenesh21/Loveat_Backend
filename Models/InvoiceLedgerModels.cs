using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M43: Invoice & GST ─────────────────────────────────────────
    public class Invoice
    {
        [Key] public int Id { get; set; }
        public string InvoiceNumber  { get; set; } = ""; // INV-2024-00001
        public int    BookingId      { get; set; }
        public int    CustomerId     { get; set; }
        public User?  Customer       { get; set; }
        public int    ChefId         { get; set; }
        public User?  Chef           { get; set; }
        public decimal SubTotal      { get; set; }
        public decimal GSTPercent    { get; set; } = 18;  // 18% GST
        public decimal GSTAmount     { get; set; }
        public decimal TotalAmount   { get; set; }
        public decimal PlatformFee   { get; set; }
        public string  Status        { get; set; } = "Generated"; // Generated / Sent / Paid
        public string  PaymentMethod { get; set; } = "";
        public string? GSTIN         { get; set; }         // customer's GST number if corporate
        public string  InvoiceDate   { get; set; } = "";
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
    }

    // ── M44: Transaction Ledger ────────────────────────────────────
    public class TransactionRecord
    {
        [Key] public int Id { get; set; }
        public string  TransactionId  { get; set; } = ""; // TXN-20241222-001
        public int?    UserId         { get; set; }
        public User?   User           { get; set; }
        public int?    BookingId      { get; set; }
        public string  Type           { get; set; } = ""; // Booking / Refund / Withdrawal / Commission / Coupon / Penalty
        public string  Direction      { get; set; } = ""; // Credit / Debit
        public decimal Amount         { get; set; }
        public decimal BalanceAfter   { get; set; }
        public string  Description    { get; set; } = "";
        public string  PaymentMethod  { get; set; } = "";
        public string  Status         { get; set; } = "Completed";
        public string? Reference      { get; set; }        // external payment ref
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M43 DTOs
    public class InvoiceDto
    {
        public int     Id            { get; set; }
        public string  InvoiceNumber { get; set; } = "";
        public int     BookingId     { get; set; }
        public string  CustomerName  { get; set; } = "";
        public string  ChefName      { get; set; } = "";
        public decimal SubTotal      { get; set; }
        public decimal GSTPercent    { get; set; }
        public decimal GSTAmount     { get; set; }
        public decimal TotalAmount   { get; set; }
        public decimal PlatformFee   { get; set; }
        public string  Status        { get; set; } = "";
        public string  PaymentMethod { get; set; } = "";
        public string? GSTIN         { get; set; }
        public string  InvoiceDate   { get; set; } = "";
        public string  CreatedAt     { get; set; } = "";
    }

    public class TaxReportDto
    {
        public string Period              { get; set; } = "";
        public decimal TotalRevenue       { get; set; }
        public decimal TotalGSTCollected  { get; set; }
        public decimal TotalSubTotal      { get; set; }
        public decimal TotalPlatformFee   { get; set; }
        public int     TotalInvoices      { get; set; }
        public List<InvoiceDto> Invoices  { get; set; } = new();
    }

    // M44 DTOs
    public class TransactionDto
    {
        public int     Id            { get; set; }
        public string  TransactionId { get; set; } = "";
        public string  UserName      { get; set; } = "";
        public int?    BookingId     { get; set; }
        public string  Type          { get; set; } = "";
        public string  Direction     { get; set; } = "";
        public decimal Amount        { get; set; }
        public decimal BalanceAfter  { get; set; }
        public string  Description   { get; set; } = "";
        public string  PaymentMethod { get; set; } = "";
        public string  Status        { get; set; } = "";
        public string? Reference     { get; set; }
        public string  CreatedAt     { get; set; } = "";
    }

    public class LedgerSummaryDto
    {
        public decimal TotalCredits      { get; set; }
        public decimal TotalDebits       { get; set; }
        public decimal NetFlow           { get; set; }
        public int     TotalTransactions { get; set; }
        public Dictionary<string, decimal> ByType { get; set; } = new();
        public List<TransactionDto> Recent         { get; set; } = new();
    }
}
