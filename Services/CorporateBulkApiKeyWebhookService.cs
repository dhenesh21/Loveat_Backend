using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M129: Enterprise — Corporate Accounts ───────────────────────
    public class CorporateAccountService
    {
        private readonly AppDbContext _db;
        public CorporateAccountService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CorporateAccountDto? Account)> CreateAsync(CreateCorporateAccountRequestDto req)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == req.OwnerUserId)) return (false, "Owner user not found.", null);
            var account = new CorporateAccount { CompanyName = req.CompanyName, BillingEmail = req.BillingEmail, Gstin = req.Gstin, CreditLimit = req.CreditLimit, OwnerUserId = req.OwnerUserId };
            _db.CorporateAccounts.Add(account);
            await _db.SaveChangesAsync();
            _db.CorporateAccountMembers.Add(new CorporateAccountMember { CorporateAccountId = account.Id, UserId = req.OwnerUserId, Role = "Owner" });
            await _db.SaveChangesAsync();
            return (true, "Corporate account created.", await ToDtoAsync(account));
        }

        public async Task<List<CorporateAccountDto>> GetAllAsync()
        {
            var accounts = await _db.CorporateAccounts.Include(a => a.OwnerUser).OrderByDescending(a => a.CreatedAt).ToListAsync();
            var result = new List<CorporateAccountDto>();
            foreach (var a in accounts) result.Add(await ToDtoAsync(a));
            return result;
        }

        public async Task<(bool Success, string Message, CorporateMemberDto? Member)> AddMemberAsync(AddCorporateMemberRequestDto req)
        {
            if (!await _db.CorporateAccounts.AnyAsync(a => a.Id == req.CorporateAccountId)) return (false, "Corporate account not found.", null);
            if (await _db.CorporateAccountMembers.AnyAsync(m => m.CorporateAccountId == req.CorporateAccountId && m.UserId == req.UserId))
                return (false, "This user is already a member.", null);
            var member = new CorporateAccountMember { CorporateAccountId = req.CorporateAccountId, UserId = req.UserId, Role = req.Role };
            _db.CorporateAccountMembers.Add(member);
            await _db.SaveChangesAsync();
            var user = await _db.Users.FindAsync(req.UserId);
            return (true, "Member added.", new CorporateMemberDto { Id = member.Id, UserId = member.UserId, UserName = user?.FullName, Role = member.Role, IsActive = member.IsActive });
        }

        public async Task<List<CorporateMemberDto>> GetMembersAsync(int corporateAccountId)
        {
            var members = await _db.CorporateAccountMembers.Include(m => m.User).Where(m => m.CorporateAccountId == corporateAccountId).ToListAsync();
            return members.Select(m => new CorporateMemberDto { Id = m.Id, UserId = m.UserId, UserName = m.User?.FullName, Role = m.Role, IsActive = m.IsActive }).ToList();
        }

        private async Task<CorporateAccountDto> ToDtoAsync(CorporateAccount a)
        {
            var owner = a.OwnerUser ?? await _db.Users.FindAsync(a.OwnerUserId);
            var memberCount = await _db.CorporateAccountMembers.CountAsync(m => m.CorporateAccountId == a.Id && m.IsActive);
            return new CorporateAccountDto
            {
                Id = a.Id, CompanyName = a.CompanyName, BillingEmail = a.BillingEmail, Gstin = a.Gstin, Status = a.Status,
                CreditLimit = a.CreditLimit, CurrentOutstanding = a.CurrentOutstanding, OwnerName = owner?.FullName, MemberCount = memberCount, CreatedAt = a.CreatedAt,
            };
        }
    }

    // ── M130: Enterprise — Bulk Ordering ────────────────────────────
    public class BulkOrderingService
    {
        private readonly AppDbContext _db;
        public BulkOrderingService(AppDbContext db) => _db = db;

        // Volume discount tiers — advisory pricing incentive for large
        // corporate orders, applied to the estimated total shown for approval.
        private static decimal DiscountFor(int mealCount) => mealCount switch { >= 200 => 12, >= 100 => 8, >= 50 => 5, >= 20 => 2, _ => 0 };

        public async Task<(bool Success, string Message, BulkOrderRequestDto? Order)> CreateAsync(CreateBulkOrderRequestDto req)
        {
            if (!await _db.CorporateAccounts.AnyAsync(a => a.Id == req.CorporateAccountId)) return (false, "Corporate account not found.", null);
            if (req.LineItems.Count == 0) return (false, "At least one line item is required.", null);

            var mealCount = req.LineItems.Sum(li => li.Quantity);
            var subtotal = req.LineItems.Sum(li => li.Quantity * li.UnitPrice);
            var discountPercent = DiscountFor(mealCount);
            var estimatedTotal = Math.Round(subtotal * (1 - discountPercent / 100), 2);

            var order = new BulkOrderRequest
            {
                CorporateAccountId = req.CorporateAccountId, RequestedByUserId = req.RequestedByUserId, City = req.City,
                MealCount = mealCount, DeliveryDate = req.DeliveryDate, EstimatedTotal = estimatedTotal,
                VolumeDiscountPercent = discountPercent > 0 ? discountPercent : null, Notes = req.Notes,
            };
            _db.BulkOrderRequests.Add(order);
            await _db.SaveChangesAsync();

            foreach (var li in req.LineItems)
                _db.BulkOrderLineItems.Add(new BulkOrderLineItem { BulkOrderRequestId = order.Id, ChefId = li.ChefId, Quantity = li.Quantity, UnitPrice = li.UnitPrice });
            await _db.SaveChangesAsync();

            return (true, "Bulk order request submitted for approval.", await ToDtoAsync(order));
        }

        public async Task<(bool Success, string Message)> DecideAsync(DecideBulkOrderRequestDto req)
        {
            var order = await _db.BulkOrderRequests.FindAsync(req.BulkOrderRequestId);
            if (order == null) return (false, "Bulk order request not found.");
            if (order.Status != "PendingApproval") return (false, $"Only PendingApproval requests can be decided (current: {order.Status}).");
            order.Status = req.Approve ? "Approved" : "Rejected";
            if (req.Approve) order.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, req.Approve ? "Bulk order approved." : "Bulk order rejected.");
        }

        // Creates a real Booking per line item once approved — this is what
        // turns the aggregated request into actual chef-facing work.
        public async Task<(bool Success, string Message, int BookingsCreated)> FulfillAsync(int bulkOrderRequestId)
        {
            var order = await _db.BulkOrderRequests.Include(o => o.CorporateAccount).FirstOrDefaultAsync(o => o.Id == bulkOrderRequestId);
            if (order == null) return (false, "Bulk order request not found.", 0);
            if (order.Status != "Approved") return (false, $"Only Approved orders can be fulfilled (current: {order.Status}).", 0);

            var lineItems = await _db.BulkOrderLineItems.Where(li => li.BulkOrderRequestId == bulkOrderRequestId && li.CreatedBookingId == null).ToListAsync();
            var created = 0;
            foreach (var li in lineItems)
            {
                var booking = new Booking
                {
                    CustomerId = order.RequestedByUserId, ChefId = li.ChefId, TotalAmount = li.UnitPrice * li.Quantity,
                    BaseAmount = li.UnitPrice * li.Quantity, ScheduledAt = order.DeliveryDate, Status = "Pending",
                };
                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync(); // need booking.Id before linking
                li.CreatedBookingId = booking.Id;
                created++;
            }
            order.Status = "Fulfilled";
            await _db.SaveChangesAsync();
            return (true, $"{created} booking(s) created.", created);
        }

        public async Task<List<BulkOrderRequestDto>> GetAllAsync()
        {
            var orders = await _db.BulkOrderRequests.Include(o => o.CorporateAccount).Include(o => o.RequestedByUser).OrderByDescending(o => o.CreatedAt).ToListAsync();
            var result = new List<BulkOrderRequestDto>();
            foreach (var o in orders) result.Add(await ToDtoAsync(o));
            return result;
        }

        private async Task<BulkOrderRequestDto> ToDtoAsync(BulkOrderRequest o)
        {
            var account = o.CorporateAccount ?? await _db.CorporateAccounts.FindAsync(o.CorporateAccountId);
            var requester = o.RequestedByUser ?? await _db.Users.FindAsync(o.RequestedByUserId);
            var lineItems = await _db.BulkOrderLineItems.Include(li => li.Chef).Where(li => li.BulkOrderRequestId == o.Id).ToListAsync();
            return new BulkOrderRequestDto
            {
                Id = o.Id, CorporateAccountId = o.CorporateAccountId, CompanyName = account?.CompanyName, RequestedByName = requester?.FullName,
                City = o.City, MealCount = o.MealCount, DeliveryDate = o.DeliveryDate, EstimatedTotal = o.EstimatedTotal,
                VolumeDiscountPercent = o.VolumeDiscountPercent, Status = o.Status, Notes = o.Notes, CreatedAt = o.CreatedAt,
                LineItems = lineItems.Select(li => new BulkOrderLineItemDto { Id = li.Id, ChefId = li.ChefId, ChefName = li.Chef?.FullName, Quantity = li.Quantity, UnitPrice = li.UnitPrice, CreatedBookingId = li.CreatedBookingId }).ToList(),
            };
        }
    }

    // ── M131: Enterprise — API Key Management ───────────────────────
    public class ApiKeyService
    {
        private readonly AppDbContext _db;
        public ApiKeyService(AppDbContext db) => _db = db;

        private static string GenerateRawKey() => "lk_live_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        private static string Hash(string raw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        public async Task<ApiKeyCreatedDto> CreateAsync(int adminUserId, CreateApiKeyRequestDto req)
        {
            var raw = GenerateRawKey();
            var key = new ApiKey
            {
                Label = req.Label, KeyPrefix = raw[..16], KeyHash = Hash(raw), Scopes = req.Scopes,
                CorporateAccountId = req.CorporateAccountId, RateLimitPerMinute = req.RateLimitPerMinute, CreatedByAdminId = adminUserId,
            };
            _db.ApiKeys.Add(key);
            await _db.SaveChangesAsync();
            // Full key is returned exactly once — only the hash is ever persisted.
            return new ApiKeyCreatedDto { Id = key.Id, Label = key.Label, FullKey = raw, KeyPrefix = key.KeyPrefix, Scopes = key.Scopes };
        }

        public async Task<List<ApiKeyDto>> GetAllAsync()
        {
            var keys = await _db.ApiKeys.Include(k => k.CorporateAccount).OrderByDescending(k => k.CreatedAt).ToListAsync();
            return keys.Select(k => new ApiKeyDto
            {
                Id = k.Id, Label = k.Label, KeyPrefix = k.KeyPrefix, Scopes = k.Scopes, CorporateAccountName = k.CorporateAccount?.CompanyName,
                RateLimitPerMinute = k.RateLimitPerMinute, IsActive = k.IsActive, LastUsedAt = k.LastUsedAt, CreatedAt = k.CreatedAt,
            }).ToList();
        }

        public async Task<(bool Success, string Message)> RevokeAsync(int apiKeyId)
        {
            var key = await _db.ApiKeys.FindAsync(apiKeyId);
            if (key == null) return (false, "API key not found.");
            key.IsActive = false;
            key.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "API key revoked.");
        }

        // Validates a raw key from an incoming request's header against the
        // stored hash — used by an API-key auth middleware (not wired into
        // the pipeline in this batch; exposed here for that future wiring).
        public async Task<ApiKey?> ValidateAsync(string rawKey)
        {
            var hash = Hash(rawKey);
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.KeyHash == hash && k.IsActive);
            if (key != null) { key.LastUsedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
            return key;
        }
    }

    // ── M132: Enterprise — Webhook Management ───────────────────────
    public class WebhookService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        public WebhookService(AppDbContext db, IHttpClientFactory httpClientFactory) { _db = db; _httpClientFactory = httpClientFactory; }

        private static string GenerateSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(20)).ToLowerInvariant();

        public async Task<WebhookCreatedDto> CreateAsync(CreateWebhookRequestDto req)
        {
            var sub = new WebhookSubscription { CorporateAccountId = req.CorporateAccountId, TargetUrl = req.TargetUrl, EventTypes = req.EventTypes, SigningSecret = GenerateSecret() };
            _db.WebhookSubscriptions.Add(sub);
            await _db.SaveChangesAsync();
            // Signing secret is returned once — used by the receiver to verify the HMAC signature on deliveries.
            return new WebhookCreatedDto { Id = sub.Id, TargetUrl = sub.TargetUrl, EventTypes = sub.EventTypes, SigningSecret = sub.SigningSecret };
        }

        public async Task<List<WebhookSubscriptionDto>> GetAllAsync()
            => (await _db.WebhookSubscriptions.OrderByDescending(s => s.CreatedAt).ToListAsync())
                .Select(s => new WebhookSubscriptionDto { Id = s.Id, TargetUrl = s.TargetUrl, EventTypes = s.EventTypes, IsActive = s.IsActive, CreatedAt = s.CreatedAt }).ToList();

        public async Task<(bool Success, string Message)> DeactivateAsync(int webhookId)
        {
            var sub = await _db.WebhookSubscriptions.FindAsync(webhookId);
            if (sub == null) return (false, "Webhook not found.");
            sub.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Webhook deactivated.");
        }

        // Delivers a real HTTP POST (best-effort, single attempt — retry
        // logic would be a follow-up) with an HMAC-SHA256 signature header
        // so receivers can verify the payload came from LovEat.
        public async Task<int> TriggerEventAsync(TriggerWebhookEventRequestDto req)
        {
            var subs = await _db.WebhookSubscriptions.Where(s => s.IsActive && s.EventTypes.Contains(req.EventType)).ToListAsync();
            var payloadJson = JsonSerializer.Serialize(new { eventType = req.EventType, data = req.Payload, sentAt = DateTime.UtcNow });
            var delivered = 0;

            foreach (var sub in subs)
            {
                var log = new WebhookDeliveryLog { WebhookSubscriptionId = sub.Id, EventType = req.EventType, Payload = payloadJson, AttemptCount = 1 };
                try
                {
                    var signature = Convert.ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(sub.SigningSecret)).ComputeHash(Encoding.UTF8.GetBytes(payloadJson)));
                    var client = _httpClientFactory.CreateClient();
                    var httpReq = new HttpRequestMessage(HttpMethod.Post, sub.TargetUrl) { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") };
                    httpReq.Headers.Add("X-LovEat-Signature", signature);
                    var resp = await client.SendAsync(httpReq);
                    log.ResponseStatusCode = (int)resp.StatusCode;
                    log.Status = resp.IsSuccessStatusCode ? "Delivered" : "Failed";
                    if (resp.IsSuccessStatusCode) { log.DeliveredAt = DateTime.UtcNow; delivered++; }
                }
                catch
                {
                    log.Status = "Failed";
                }
                _db.WebhookDeliveryLogs.Add(log);
            }
            await _db.SaveChangesAsync();
            return delivered;
        }

        public async Task<List<WebhookDeliveryLogDto>> GetDeliveryLogAsync(int webhookId)
        {
            var logs = await _db.WebhookDeliveryLogs.Where(l => l.WebhookSubscriptionId == webhookId).OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
            return logs.Select(l => new WebhookDeliveryLogDto { Id = l.Id, EventType = l.EventType, Status = l.Status, ResponseStatusCode = l.ResponseStatusCode, AttemptCount = l.AttemptCount, CreatedAt = l.CreatedAt, DeliveredAt = l.DeliveredAt }).ToList();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/corporate-accounts"), Authorize(Roles = "Admin")]
    public class CorporateAccountController : ControllerBase
    {
        private readonly Services.CorporateAccountService _svc;
        public CorporateAccountController(Services.CorporateAccountService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCorporateAccountRequestDto req)
        {
            var (success, message, account) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = account });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("members")]
        public async Task<IActionResult> AddMember([FromBody] AddCorporateMemberRequestDto req)
        {
            var (success, message, member) = await _svc.AddMemberAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = member });
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id) => Ok(new { success = true, data = await _svc.GetMembersAsync(id) });
    }

    [ApiController, Route("api/bulk-ordering")]
    public class BulkOrderingController : ControllerBase
    {
        private readonly Services.BulkOrderingService _svc;
        public BulkOrderingController(Services.BulkOrderingService svc) => _svc = svc;

        [HttpPost, Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBulkOrderRequestDto req)
        {
            var (success, message, order) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = order });
        }

        [HttpPost("decide"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Decide([FromBody] DecideBulkOrderRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("fulfill"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Fulfill([FromBody] FulfillBulkOrderRequestDto req)
        {
            var (success, message, count) = await _svc.FulfillAsync(req.BulkOrderRequestId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, bookingsCreated = count });
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/api-keys"), Authorize(Roles = "Admin")]
    public class ApiKeyController : ControllerBase
    {
        private readonly Services.ApiKeyService _svc;
        public ApiKeyController(Services.ApiKeyService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeApiKeyRequestDto req)
        {
            var (success, message) = await _svc.RevokeAsync(req.ApiKeyId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/webhooks"), Authorize(Roles = "Admin")]
    public class WebhookController : ControllerBase
    {
        private readonly Services.WebhookService _svc;
        public WebhookController(Services.WebhookService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWebhookRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var (success, message) = await _svc.DeactivateAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger([FromBody] TriggerWebhookEventRequestDto req) => Ok(new { success = true, delivered = await _svc.TriggerEventAsync(req) });

        [HttpGet("{id}/deliveries")]
        public async Task<IActionResult> GetDeliveries(int id) => Ok(new { success = true, data = await _svc.GetDeliveryLogAsync(id) });
    }
}
