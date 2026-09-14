namespace LovEat.API.DTOs
{
    // ── Loyalty DTOs ───────────────────────────────────────────────
    public class LoyaltyStatusDto
    {
        public int    TotalPoints     { get; set; }
        public int    AvailablePoints { get; set; }
        public int    RedeemedPoints  { get; set; }
        public int    PendingPoints   { get; set; }
        public string Tier            { get; set; } = "";
        public int    PointsToNextTier{ get; set; }
        public string NextTier        { get; set; } = "";
        public double TierProgress    { get; set; }   // 0.0–1.0
        public List<LoyaltyTransactionDto> RecentTransactions { get; set; } = new();
        public List<LoyaltyRewardDto>      AvailableRewards   { get; set; } = new();
    }

    public class LoyaltyTransactionDto
    {
        public int    Id          { get; set; }
        public int    Points      { get; set; }
        public string Type        { get; set; } = "";
        public string Reason      { get; set; } = "";
        public int    BalanceAfter{ get; set; }
        public string CreatedAt   { get; set; } = "";
    }

    public class LoyaltyRewardDto
    {
        public int    Id           { get; set; }
        public string Title        { get; set; } = "";
        public string Description  { get; set; } = "";
        public int    PointsCost   { get; set; }
        public string RewardType   { get; set; } = ""; // Discount / FreeBooking / CashBack
        public decimal Value       { get; set; }        // ₹ value or % discount
        public bool   CanRedeem    { get; set; }
    }

    public class RedeemPointsRequestDto
    {
        public int RewardId     { get; set; }
        public int PointsToUse  { get; set; }
    }

    public class RedeemPointsResponseDto
    {
        public bool   Success        { get; set; }
        public string Message        { get; set; } = "";
        public string CouponCode     { get; set; } = "";
        public int    PointsDeducted { get; set; }
        public int    NewBalance     { get; set; }
    }

    // Admin DTOs
    public class AdminLoyaltyStatsDto
    {
        public int     TotalUsersWithPoints  { get; set; }
        public long    TotalPointsIssued     { get; set; }
        public long    TotalPointsRedeemed   { get; set; }
        public long    TotalPointsExpired    { get; set; }
        public decimal PointsValueRedeemed   { get; set; }
        public List<TierDistributionDto> TierBreakdown { get; set; } = new();
        public List<LoyaltyLeaderDto>    TopUsers      { get; set; } = new();
    }

    public class TierDistributionDto
    {
        public string Tier    { get; set; } = "";
        public int    Count   { get; set; }
        public double Percent { get; set; }
    }

    public class LoyaltyLeaderDto
    {
        public int    UserId   { get; set; }
        public string UserName { get; set; } = "";
        public string Phone    { get; set; } = "";
        public int    Points   { get; set; }
        public string Tier     { get; set; } = "";
    }

    // ── Referral DTOs ──────────────────────────────────────────────
    public class ReferralStatusDto
    {
        public string ReferralCode        { get; set; } = "";
        public string ReferralLink        { get; set; } = "";
        public int    TotalReferrals      { get; set; }
        public int    SuccessfulReferrals { get; set; }
        public int    PendingReferrals    { get; set; }
        public int    PointsEarned        { get; set; }
        public int    PointsPerReferral   { get; set; }
        public List<ReferralUseDto> RecentReferrals { get; set; } = new();
    }

    public class ReferralUseDto
    {
        public string ReferredUserName { get; set; } = "";
        public string Status           { get; set; } = "";
        public int    Points           { get; set; }
        public string UsedAt           { get; set; } = "";
    }

    public class ApplyReferralRequestDto
    {
        public string Code { get; set; } = "";
    }

    public class ApplyReferralResponseDto
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = "";
        public int    BonusPoints { get; set; }
    }

    // Admin Referral
    public class AdminReferralStatsDto
    {
        public int TotalCodes      { get; set; }
        public int TotalReferrals  { get; set; }
        public int CompletedReferrals { get; set; }
        public int PointsAwarded   { get; set; }
        public List<ReferralLeaderDto> TopReferrers { get; set; } = new();
    }

    public class ReferralLeaderDto
    {
        public int    UserId       { get; set; }
        public string UserName     { get; set; } = "";
        public string Code         { get; set; } = "";
        public int    Referrals    { get; set; }
        public int    PointsEarned { get; set; }
    }
}
