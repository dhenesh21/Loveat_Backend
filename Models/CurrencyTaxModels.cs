using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M73: Multi-currency support
    //
    // Scope note: all money is still stored, calculated, and settled in INR
    // throughout the platform (bookings, payments, commissions, invoices —
    // none of that changes). This is *display-currency* conversion only, so
    // a customer browsing from outside India sees a familiar currency
    // without touching the pricing/settlement logic everywhere else.
    // Exchange rates are seeded with representative fixed values and
    // updated manually via the admin endpoint — TODO (new gap, call it G9 — G8 is already "Tests and CI/CD" in the original checklist):
    // wire a live FX rate API (e.g. exchangerate-api.com) to refresh these
    // automatically; right now they will drift from real market rates.
    // ══════════════════════════════════════════════════════════════
    public class Currency
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(5)]
        public string Code { get; set; } = ""; // ISO 4217: INR, USD, EUR, GBP, AED

        [MaxLength(5)]
        public string Symbol { get; set; } = "";

        [MaxLength(30)]
        public string Name { get; set; } = "";

        /// <summary>How many units of this currency equal 1 INR. INR itself is always 1.0.</summary>
        public decimal ExchangeRateToINR { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M74: Multi-country tax compliance
    //
    // Generalizes batch 22's Invoice.GSTPercent, which is hardcoded to 18%
    // for India-only GST. This table lets a rate be looked up per
    // country/region instead — TaxCalculationService is additive (new
    // endpoints), it does NOT change Invoice/InvoiceService's existing
    // behavior, since that's already-working India billing code. Wiring
    // InvoiceService to call this instead of its hardcoded rate is a
    // follow-up once real multi-country billing is actually needed.
    // ══════════════════════════════════════════════════════════════
    public class TaxRule
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(60)]
        public string Country { get; set; } = "";

        /// <summary>State/province, for countries where tax varies by region (e.g. US sales tax). Null = country-wide rate.</summary>
        [MaxLength(60)]
        public string? Region { get; set; }

        [MaxLength(20)]
        public string TaxName { get; set; } = ""; // GST / VAT / SalesTax

        public decimal Rate { get; set; } // percent, e.g. 5 = 5%

        [MaxLength(20)]
        public string AppliesTo { get; set; } = "All"; // Booking / Subscription / All

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
