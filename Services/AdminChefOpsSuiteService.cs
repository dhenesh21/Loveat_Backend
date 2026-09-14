using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // AdminChefBusinessSuiteOverview
    // ══════════════════════════════════════════════════════════════
    public class ChefBusinessSuiteService
    {
        private readonly AppDbContext _db;
        public ChefBusinessSuiteService(AppDbContext db) => _db = db;

        public async Task<ChefBusinessSuiteOverviewDto> GetOverviewAsync()
        {
            var lowStock = await _db.ChefInventoryItems
                .Where(i => i.QuantityOnHand <= i.LowStockThreshold)
                .Include(i => i.Chef)
                .OrderBy(i => i.QuantityOnHand)
                .Take(20)
                .Select(i => new LowStockItemDto
                {
                    Id = i.Id,
                    ChefName = i.Chef != null ? i.Chef.FullName : "",
                    ItemName = i.ItemName,
                    QuantityOnHand = i.QuantityOnHand,
                    LowStockThreshold = i.LowStockThreshold,
                    Unit = i.Unit
                }).ToListAsync();

            var staffSummary = await _db.ChefStaffMembers
                .Include(s => s.Chef)
                .GroupBy(s => new { s.ChefId, ChefName = s.Chef != null ? s.Chef.FullName : "" })
                .Select(g => new StaffSummaryDto
                {
                    ChefName = g.Key.ChefName,
                    ActiveStaffCount = g.Count(s => s.Active),
                    TotalStaffCount = g.Count()
                }).Take(20).ToListAsync();

            var packages = await _db.ChefPackages
                .Include(p => p.Chef)
                .OrderByDescending(p => p.TimesBooked)
                .Take(20)
                .Select(p => new ChefPackageDto
                {
                    Id = p.Id,
                    ChefName = p.Chef != null ? p.Chef.FullName : "",
                    Name = p.Name,
                    Price = p.Price,
                    TimesBooked = p.TimesBooked,
                    Active = p.Active
                }).ToListAsync();

            // Promo usage — real coupon usage logs (Batch: CouponTrackingController).
            var promoUsage = await _db.CouponUsages
                .Include(r => r.Coupon)
                .GroupBy(r => r.Coupon != null ? r.Coupon.Code : "UNKNOWN")
                .Select(g => new PromoUsageDto
                {
                    Code = g.Key,
                    TimesUsed = g.Count(),
                    TotalDiscountGiven = g.Sum(r => r.DiscountApplied)
                })
                .OrderByDescending(p => p.TimesUsed)
                .Take(10)
                .ToListAsync();

            // Tax estimates — reuse the pre-existing ChefTaxEstimate table (Section 44AD
            // presumptive-tax model); latest estimate per chef. Empty until a chef generates one.
            var taxEstimates = await _db.ChefTaxEstimates
                .Include(t => t.Chef)
                .GroupBy(t => t.ChefId)
                .Select(g => g.OrderByDescending(t => t.GeneratedAt).First())
                .OrderByDescending(t => t.GrossIncome)
                .Take(15)
                .Select(t => new TaxEstimateDto
                {
                    ChefName = t.Chef != null ? t.Chef.FullName : "",
                    FinancialYear = t.FinancialYear,
                    GrossEarnings = t.GrossIncome,
                    EstimatedTax = t.EstimatedTaxLiability
                })
                .ToListAsync();

            // Goals — reuse the pre-existing ChefBusinessGoal table.
            var totalGoals = await _db.ChefBusinessGoals.CountAsync();
            var achieved = await _db.ChefBusinessGoals.CountAsync(g => g.ActualValue >= g.TargetValue);

            // Outlets — reuse the pre-existing ChefOutlet table.
            var outlets = await _db.ChefOutlets
                .Include(o => o.Chef)
                .OrderBy(o => o.ChefId).ThenByDescending(o => o.IsPrimary)
                .Take(30)
                .Select(o => new AdminOpsChefOutletDto
                {
                    ChefName = o.Chef != null ? o.Chef.FullName : "",
                    OutletName = o.OutletName,
                    OutletType = o.OutletType,
                    IsPrimary = o.IsPrimary
                }).ToListAsync();

            return new ChefBusinessSuiteOverviewDto
            {
                LowStock = lowStock,
                StaffSummary = staffSummary,
                Packages = packages,
                PromoUsage = promoUsage,
                TaxEstimates = taxEstimates,
                GoalsAchieved = new GoalsAchievedSummaryDto
                {
                    TotalGoalsSet = totalGoals,
                    Achieved = achieved,
                    AchievedPercent = totalGoals == 0 ? 0 : Math.Round(achieved * 100.0 / totalGoals, 1)
                },
                Outlets = outlets
            };
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminChefPerformanceAnalytics page — real-time computation from
    // Booking/Review data (response time, repeat-customer rate, grade,
    // rank). Named "Overview" to avoid colliding with the pre-existing
    // M123 ChefPerformanceAnalyticsService (snapshot/leaderboard model,
    // route api/chef-performance-analytics) — different mechanism,
    // same general subject, kept separate rather than merged.
    // ══════════════════════════════════════════════════════════════
    public class ChefPerformanceOverviewService
    {
        private readonly AppDbContext _db;
        public ChefPerformanceOverviewService(AppDbContext db) => _db = db;

        public async Task<List<AdminOpsChefPerformanceDto>> GetAllAsync()
        {
            var chefs = await _db.Users.Where(u => u.Role == "Chef").ToListAsync();
            var profiles = await _db.ChefProfiles.ToDictionaryAsync(p => p.UserId, p => p.City ?? "");
            var result = new List<AdminOpsChefPerformanceDto>();

            foreach (var chef in chefs)
            {
                var bookings = await _db.Bookings.Where(b => b.ChefId == chef.Id)
                    .OrderBy(b => b.CreatedAt).ToListAsync();
                var reviews = await _db.Reviews.Where(r => r.ChefId == chef.Id).ToListAsync();
                var completed = bookings.Count(b => b.Status == "Completed");
                var cancelled = bookings.Count(b => b.Status == "Cancelled");

                var responseTimes = bookings
                    .Where(b => b.AcceptedAt.HasValue)
                    .Select(b => (b.AcceptedAt!.Value - b.CreatedAt).TotalMinutes)
                    .Where(m => m >= 0).ToList();

                var custGroups = bookings.GroupBy(b => b.CustomerId).ToList();
                var uniqueCustomers = custGroups.Count;
                var repeatCustomers = custGroups.Count(g => g.Count() > 1);

                var avgRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 2) : 0;
                var completionRate = bookings.Count == 0 ? 0 : Math.Round(completed * 100.0 / bookings.Count, 1);

                var trend = "Stable";
                if (bookings.Count >= 6)
                {
                    var half = bookings.Count / 2;
                    var earlier = bookings.Take(half).ToList();
                    var recent = bookings.Skip(half).ToList();
                    double RateOf(List<Booking> list) => list.Count == 0 ? 0 : list.Count(b => b.Status == "Completed") * 100.0 / list.Count;
                    var diff = RateOf(recent) - RateOf(earlier);
                    trend = diff > 5 ? "Improving" : diff < -5 ? "Declining" : "Stable";
                }

                var grade = avgRating >= 4.8 && completionRate >= 90 ? "A+"
                    : avgRating >= 4.5 && completionRate >= 80 ? "A"
                    : avgRating >= 4.0 && completionRate >= 70 ? "B"
                    : avgRating >= 3.5 && completionRate >= 55 ? "C"
                    : "D";

                result.Add(new AdminOpsChefPerformanceDto
                {
                    ChefUserId = chef.Id,
                    ChefName = chef.FullName,
                    City = profiles.TryGetValue(chef.Id, out var city) ? city : "",
                    AvgRating = avgRating,
                    TotalReviews = reviews.Count,
                    TotalBookings = bookings.Count,
                    CompletedBookings = completed,
                    CancelledBookings = cancelled,
                    CompletionRate = completionRate,
                    AvgResponseTimeMins = responseTimes.Any() ? Math.Round(responseTimes.Average(), 1) : 0,
                    TotalEarnings = bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                    TotalHoursWorked = Math.Round(bookings.Where(b => b.Status == "Completed").Sum(b => b.DurationMinutes) / 60.0, 1),
                    UniqueCustomers = uniqueCustomers,
                    RepeatCustomers = repeatCustomers,
                    RepeatRate = uniqueCustomers == 0 ? 0 : Math.Round(repeatCustomers * 100.0 / uniqueCustomers, 1),
                    PerformanceGrade = grade,
                    Trend = trend
                });
            }

            var byEarnings = result.OrderByDescending(c => c.TotalEarnings).Select((c, i) => (c, i)).ToList();
            foreach (var (c, i) in byEarnings) c.EarningsRank = i + 1;
            var byRating = result.OrderByDescending(c => c.AvgRating).Select((c, i) => (c, i)).ToList();
            foreach (var (c, i) in byRating) c.RatingRank = i + 1;
            var byCompletion = result.OrderByDescending(c => c.CompletionRate).Select((c, i) => (c, i)).ToList();
            foreach (var (c, i) in byCompletion) c.CompletionRank = i + 1;

            var cityCounts = result.GroupBy(c => c.City).ToDictionary(g => g.Key, g => g.Count());
            foreach (var c in result) c.TotalChefsInCity = cityCounts.TryGetValue(c.City, out var n) ? n : 1;

            return result.OrderByDescending(c => c.TotalEarnings).ToList();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminEquipmentCertifications — reuses the pre-existing
    // EquipmentListing / CertificationCourse / ChefCertification
    // models (schema already migrated; no service/controller existed).
    // ══════════════════════════════════════════════════════════════
    public class EquipmentCertificationService
    {
        private readonly AppDbContext _db;
        public EquipmentCertificationService(AppDbContext db) => _db = db;

        public async Task<List<AdminOpsEquipmentListingDto>> GetListingsAsync()
        {
            return await _db.EquipmentListings
                .Include(l => l.Chef)
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .Select(l => new AdminOpsEquipmentListingDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    ChefName = l.Chef != null ? l.Chef.FullName : "",
                    Category = l.Category,
                    ListingType = l.ListingType,
                    Price = l.Price,
                    IsAvailable = l.IsAvailable,
                    City = l.City
                }).ToListAsync();
        }

        public async Task<AdminOpsEquipmentListingDto> CreateListingAsync(CreateEquipmentListingRequestDto req)
        {
            var listing = new EquipmentListing
            {
                ChefId = req.ChefUserId,
                Title = req.Title,
                Category = req.Category,
                ListingType = req.ListingType,
                Price = req.Price,
                PriceUnit = req.ListingType == "Rent" ? "per day" : "fixed",
                City = req.City,
                IsAvailable = true
            };
            _db.EquipmentListings.Add(listing);
            await _db.SaveChangesAsync();

            var chef = await _db.Users.FindAsync(req.ChefUserId);
            return new AdminOpsEquipmentListingDto
            {
                Id = listing.Id,
                Title = listing.Title,
                ChefName = chef?.FullName ?? "",
                Category = listing.Category,
                ListingType = listing.ListingType,
                Price = listing.Price,
                IsAvailable = listing.IsAvailable,
                City = listing.City
            };
        }

        public async Task<List<AdminOpsCertificationCourseDto>> GetCoursesAsync()
        {
            return await _db.CertificationCourses
                .Where(c => c.IsActive)
                .Select(c => new AdminOpsCertificationCourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Category = c.Category,
                    Level = c.Level,
                    Fee = c.Fee,
                    Enrolled = _db.ChefCertifications.Count(e => e.CourseId == c.Id),
                    Completed = _db.ChefCertifications.Count(e => e.CourseId == c.Id && e.Status == "Completed")
                }).ToListAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminCorporateManager — reuses pre-existing CorporateSubscription
    // + CorporatePlan (already migrated, customer-side controller exists
    // at api/corporate-plans; this adds the missing ADMIN list/manage view)
    // and CorporateTiffinBooking (for assigned-chef lookup).
    // ══════════════════════════════════════════════════════════════
    public class CorporateManagerService
    {
        private readonly AppDbContext _db;
        public CorporateManagerService(AppDbContext db) => _db = db;

        public async Task<List<CorporateClientDto>> GetClientsAsync()
        {
            var subs = await _db.CorporateSubscriptions
                .Include(s => s.Plan)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            var chefByCompany = await _db.CorporateTiffinBookings
                .Include(b => b.PrimaryChef)
                .GroupBy(b => b.CompanyName)
                .Select(g => g.OrderByDescending(b => b.StartDate).First())
                .ToDictionaryAsync(b => b.CompanyName, b => b.PrimaryChef != null ? b.PrimaryChef.FullName : "");

            return subs.Select(s => new CorporateClientDto
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Phone = s.ContactPhone,
                Employees = s.EmployeeCount,
                Plan = s.Plan != null ? s.Plan.PlanName : "",
                MealsPerDay = s.Plan != null ? s.Plan.MealsPerDay : 0,
                Status = s.Status,
                TotalSpend = s.TotalPaid,
                StartDate = s.StartDate,
                ChefName = chefByCompany.TryGetValue(s.CompanyName, out var chef) ? chef : ""
            }).ToList();
        }

        public async Task<List<CorporatePlanSummaryDto>> GetPlansAsync()
        {
            var plans = await _db.CorporatePlans.ToListAsync();
            var result = new List<CorporatePlanSummaryDto>();
            foreach (var p in plans)
            {
                List<string> features;
                try { features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.Features) ?? new(); }
                catch { features = new(); }

                result.Add(new CorporatePlanSummaryDto
                {
                    Name = p.PlanName,
                    MealsPerDay = p.MealsPerDay,
                    PricePerMeal = p.PricePerMeal,
                    Features = features,
                    ActiveSubscribers = await _db.CorporateSubscriptions.CountAsync(s => s.CorporatePlanId == p.Id && s.Status == "Active")
                });
            }
            return result;
        }

        public async Task<bool> UpdateStatusAsync(int subscriptionId, string status)
        {
            var sub = await _db.CorporateSubscriptions.FindAsync(subscriptionId);
            if (sub == null) return false;
            sub.Status = status;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminDietarySocialPages — admin-wide views on top of the
    // pre-existing DietaryMatchingService (api/dietary, self-service)
    // and SocialService (api/social, self-service) — reuses their
    // logic/tables rather than duplicating the match-scoring algorithm.
    // ══════════════════════════════════════════════════════════════
    public class AdminDietarySocialService
    {
        private readonly AppDbContext _db;
        private readonly DietaryMatchingService _dietarySvc;
        public AdminDietarySocialService(AppDbContext db, DietaryMatchingService dietarySvc)
        {
            _db = db;
            _dietarySvc = dietarySvc;
        }

        public async Task<List<AdminUserDietaryRowDto>> GetUserProfilesAsync()
        {
            var profiles = await _db.UserDietaryProfiles.Include(p => p.User).Where(p => p.IsActive).Take(50).ToListAsync();
            var result = new List<AdminUserDietaryRowDto>();
            foreach (var p in profiles)
            {
                var matches = await _dietarySvc.GetMatchedChefsAsync(p.UserId);
                var top = matches.OrderByDescending(m => m.MatchScore).FirstOrDefault();
                result.Add(new AdminUserDietaryRowDto
                {
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.FullName : "",
                    DietType = p.DietType,
                    Allergies = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.Allergies) ?? new(),
                    HealthGoals = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.HealthGoals) ?? new(),
                    TopMatchChefName = top?.ChefName ?? "",
                    TopMatchScore = top != null ? (double)top.MatchScore : 0,
                    TotalMatches = matches.Count(m => m.MatchScore >= 60)
                });
            }
            return result;
        }

        public async Task<List<AdminMealPostDto>> GetPostsAsync()
        {
            var posts = await _db.MealPosts
                .Include(p => p.Chef)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToListAsync();

            return posts.Select(p => new AdminMealPostDto
            {
                Id = p.Id,
                ChefName = p.Chef != null ? p.Chef.FullName : "",
                Cuisine = p.Cuisine,
                Caption = p.Caption,
                Tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.Tags) ?? new(),
                LikeCount = p.LikeCount,
                CommentCount = p.CommentCount,
                IsPublic = p.IsPublic,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminChefFollowDto>> GetFollowsAsync()
        {
            return await _db.ChefFollows
                .Include(f => f.Follower).Include(f => f.Chef)
                .OrderByDescending(f => f.FollowedAt)
                .Take(50)
                .Select(f => new AdminChefFollowDto
                {
                    FollowerName = f.Follower != null ? f.Follower.FullName : "",
                    ChefName = f.Chef != null ? f.Chef.FullName : "",
                    FollowedAt = f.FollowedAt
                }).ToListAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminGroupBookingMLMatching — admin view over pre-existing
    // GroupBooking/GroupBookingSplit (M69) and ChefMatchScore cache (M70).
    // ══════════════════════════════════════════════════════════════
    public class AdminGroupMLService
    {
        private readonly AppDbContext _db;
        public AdminGroupMLService(AppDbContext db) => _db = db;

        public async Task<List<AdminGroupBookingRowDto>> GetGroupBookingsAsync()
        {
            var groups = await _db.GroupBookings
                .Include(g => g.Customer)
                .OrderByDescending(g => g.CreatedAt)
                .Take(50)
                .ToListAsync();

            var chefCounts = await _db.GroupBookingSplits
                .GroupBy(s => s.GroupBookingId)
                .Select(g => new { GroupBookingId = g.Key, Count = g.Select(x => x.ChefId).Distinct().Count() })
                .ToDictionaryAsync(x => x.GroupBookingId, x => x.Count);

            return groups.Select(g => new AdminGroupBookingRowDto
            {
                Id = g.Id,
                Customer = g.Customer != null ? g.Customer.FullName : "",
                TotalGuests = g.TotalGuestCount,
                ChefsUsed = chefCounts.TryGetValue(g.Id, out var n) ? n : 0,
                Status = g.Status,
                TotalAmount = g.TotalAmount,
                ScheduledAt = g.ScheduledAt
            }).ToList();
        }

        public async Task<AdminMLMatchStatsDto> GetMLStatsAsync()
        {
            var scores = await _db.ChefMatchScores.ToListAsync();
            if (scores.Count == 0)
                return new AdminMLMatchStatsDto { TotalMatchesComputed = 0, AvgScore = 0, TopReason = "No matches computed yet" };

            var cuisineHits = scores.Count(s => s.FactorsJson != null && s.FactorsJson.Contains("\"cuisineMatch\":true"));
            var repeatHits = scores.Count(s => s.FactorsJson != null && s.FactorsJson.Contains("\"repeatBookings\""));
            var ratingHits = scores.Count(s => s.FactorsJson != null && s.FactorsJson.Contains("\"avgRatingGiven\""));

            var topReason = cuisineHits >= repeatHits && cuisineHits >= ratingHits ? "Matches cuisines the customer books most often"
                : repeatHits >= ratingHits ? "Customer has booked this chef before"
                : "Customer rated this chef highly in the past";

            return new AdminMLMatchStatsDto
            {
                TotalMatchesComputed = scores.Count,
                AvgScore = Math.Round((double)scores.Average(s => s.Score), 1),
                TopReason = topReason
            };
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminTrackingWeatherContractMenuBuild — admin-wide views on top
    // of pre-existing OrderTrackingEvent/DigitalContract/CustomMenuBuild
    // (self-service controllers only expose per-booking/per-customer
    // lookups; this adds the missing admin list views).
    // ══════════════════════════════════════════════════════════════
    public class AdminTrackingWeatherContractMenuBuildService
    {
        private readonly AppDbContext _db;
        public AdminTrackingWeatherContractMenuBuildService(AppDbContext db) => _db = db;

        public async Task<List<AdminTrackingRowDto>> GetTrackingAsync()
        {
            var latest = await _db.OrderTrackingEvents
                .GroupBy(e => e.BookingId)
                .Select(g => g.OrderByDescending(e => e.OccurredAt).First())
                .OrderByDescending(e => e.OccurredAt)
                .Take(50)
                .ToListAsync();

            return latest.Select(e => new AdminTrackingRowDto
            {
                BookingId = e.BookingId,
                CurrentStage = e.Stage,
                LastUpdate = e.OccurredAt
            }).ToList();
        }

        public async Task<List<AdminContractRowDto>> GetContractsAsync()
        {
            return await _db.DigitalContracts
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)
                .Select(c => new AdminContractRowDto
                {
                    BookingId = c.BookingId,
                    CustomerAccepted = c.CustomerAccepted,
                    ChefAccepted = c.ChefAccepted,
                    Status = c.Status
                }).ToListAsync();
        }

        public async Task<List<AdminMenuBuildRowDto>> GetMenuBuildsAsync()
        {
            return await _db.CustomMenuBuilds
                .Include(m => m.Customer).Include(m => m.Chef)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .Select(m => new AdminMenuBuildRowDto
                {
                    CustomerName = m.Customer != null ? m.Customer.FullName : "",
                    ChefName = m.Chef != null ? m.Chef.FullName : "",
                    BuildName = m.BuildName,
                    EstimatedTotal = m.EstimatedTotal,
                    Status = m.Status
                }).ToListAsync();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // AdminWishlistBadgePreferencesReorder — admin-wide aggregates on
    // top of pre-existing WishlistItem/BadgeDefinition/CustomerBadge/
    // CustomerPreferences/QuickReorderCombo tables.
    // ══════════════════════════════════════════════════════════════
    public class AdminWishlistBadgePreferencesReorderService
    {
        private readonly AppDbContext _db;
        public AdminWishlistBadgePreferencesReorderService(AppDbContext db) => _db = db;

        public async Task<AdminWishlistStatsDto> GetWishlistStatsAsync()
        {
            var totalItems = await _db.WishlistItems.CountAsync();

            var chefWishlists = await _db.WishlistItems
                .Where(w => w.ItemType == "Chef")
                .GroupBy(w => w.ItemId)
                .Select(g => new { ChefProfileId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var profileIds = chefWishlists.Select(c => c.ChefProfileId).ToList();
            var chefNames = await _db.ChefProfiles.Include(p => p.User)
                .Where(p => profileIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.User != null ? p.User.FullName : "");

            return new AdminWishlistStatsDto
            {
                TotalItems = totalItems,
                TopChefs = chefWishlists.Select(c => new AdminWishlistTopChefDto
                {
                    ChefName = chefNames.TryGetValue(c.ChefProfileId, out var n) ? n : "",
                    WishlistCount = c.Count
                }).ToList()
            };
        }

        public async Task<List<AdminBadgeRowDto>> GetBadgesAsync()
        {
            var defs = await _db.BadgeDefinitions.Where(b => b.IsActive).ToListAsync();
            var result = new List<AdminBadgeRowDto>();
            foreach (var d in defs)
            {
                result.Add(new AdminBadgeRowDto
                {
                    Key = d.Key,
                    Name = d.Name,
                    Tier = d.Tier,
                    AwardedCount = await _db.CustomerBadges.CountAsync(cb => cb.BadgeDefinitionId == d.Id)
                });
            }
            return result;
        }

        public async Task<AdminPreferencesStatsDto> GetPreferencesStatsAsync()
        {
            var prefs = await _db.CustomerPreferences.ToListAsync();
            if (prefs.Count == 0) return new AdminPreferencesStatsDto();

            double Pct(Func<Models.CustomerPreferences, bool> pred) => Math.Round(prefs.Count(pred) * 100.0 / prefs.Count, 1);

            return new AdminPreferencesStatsDto
            {
                PushEnabledPercent = Pct(p => p.PushNotificationsEnabled),
                EmailEnabledPercent = Pct(p => p.EmailNotificationsEnabled),
                PromoOptOutPercent = Pct(p => !p.PromoNotificationsEnabled),
                DarkModePercent = Pct(p => p.Theme == "Dark")
            };
        }

        public async Task<List<AdminReorderComboRowDto>> GetTopCombosAsync()
        {
            return await _db.QuickReorderCombos
                .Include(c => c.Customer)
                .OrderByDescending(c => c.UseCount)
                .Take(15)
                .Select(c => new AdminReorderComboRowDto
                {
                    CustomerName = c.Customer != null ? c.Customer.FullName : "",
                    ComboName = c.ComboName,
                    UseCount = c.UseCount
                }).ToListAsync();
        }
    }
}