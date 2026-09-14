namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M73: Multi-currency
    // ══════════════════════════════════════════════════════════════
    public class CurrencyDto
    {
        public string Code { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal ExchangeRateToINR { get; set; }
        public bool IsActive { get; set; }
    }

    public class ConvertAmountRequestDto
    {
        public decimal AmountInINR { get; set; }
        public string ToCurrency { get; set; } = "USD";
    }

    public class ConvertAmountResponseDto
    {
        public decimal OriginalAmount { get; set; }
        public string OriginalCurrency { get; set; } = "INR";
        public decimal ConvertedAmount { get; set; }
        public string ToCurrency { get; set; } = "";
        public decimal ExchangeRate { get; set; }
        public bool IsStubRate { get; set; } = true; // flips to false once G9 (live FX API) is wired
    }

    public class SetPreferredCurrencyRequestDto
    {
        public string Code { get; set; } = "INR";
    }

    public class UpdateExchangeRateRequestDto
    {
        public string Code { get; set; } = "";
        public decimal NewRate { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M74: Multi-country tax
    // ══════════════════════════════════════════════════════════════
    public class TaxRuleDto
    {
        public int Id { get; set; }
        public string Country { get; set; } = "";
        public string? Region { get; set; }
        public string TaxName { get; set; } = "";
        public decimal Rate { get; set; }
        public string AppliesTo { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class UpsertTaxRuleRequestDto
    {
        public int? Id { get; set; } // present = update
        public string Country { get; set; } = "";
        public string? Region { get; set; }
        public string TaxName { get; set; } = "";
        public decimal Rate { get; set; }
        public string AppliesTo { get; set; } = "All";
    }

    public class CalculateTaxRequestDto
    {
        public decimal Amount { get; set; }
        public string Country { get; set; } = "India";
        public string? Region { get; set; }
    }

    public class TaxCalculationResultDto
    {
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string TaxName { get; set; } = "";
        public string Country { get; set; } = "";
        public bool RuleFound { get; set; }
    }
}
