using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M137: Enterprise — SSO / Single Sign-On ─────────────────────
    public class SsoConnectionService
    {
        private readonly AppDbContext _db;
        public SsoConnectionService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, SsoConnectionDto? Connection)> CreateAsync(CreateSsoConnectionRequestDto req)
        {
            if (!await _db.CorporateAccounts.AnyAsync(a => a.Id == req.CorporateAccountId)) return (false, "Corporate account not found.", null);
            if (await _db.SsoConnections.AnyAsync(c => c.EmailDomain == req.EmailDomain)) return (false, $"Domain '{req.EmailDomain}' is already linked to an SSO connection.", null);

            var conn = new SsoConnection
            {
                CorporateAccountId = req.CorporateAccountId, Protocol = req.Protocol, IdpEntityId = req.IdpEntityId,
                IdpSsoUrl = req.IdpSsoUrl, IdpCertificate = req.IdpCertificate, EmailDomain = req.EmailDomain.ToLowerInvariant(),
            };
            _db.SsoConnections.Add(conn);
            await _db.SaveChangesAsync();
            return (true, "SSO connection created (inactive until tested and activated).", await ToDtoAsync(conn));
        }

        public async Task<List<SsoConnectionDto>> GetAllAsync()
        {
            var conns = await _db.SsoConnections.Include(c => c.CorporateAccount).OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<SsoConnectionDto>();
            foreach (var c in conns) result.Add(await ToDtoAsync(c));
            return result;
        }

        public async Task<(bool Success, string Message)> SetActiveAsync(ActivateSsoConnectionRequestDto req)
        {
            var conn = await _db.SsoConnections.FindAsync(req.SsoConnectionId);
            if (conn == null) return (false, "SSO connection not found.");
            conn.IsActive = req.Activate;
            await _db.SaveChangesAsync();
            return (true, req.Activate ? "SSO connection activated." : "SSO connection deactivated.");
        }

        // Called at login time: if the user's email domain matches an
        // active SSO connection, the client should redirect to the IdP
        // instead of showing the password field.
        public async Task<SsoLookupResultDto> LookupByEmailAsync(string email)
        {
            var domain = email.Contains('@') ? email.Split('@')[1].ToLowerInvariant() : "";
            var conn = await _db.SsoConnections.FirstOrDefaultAsync(c => c.EmailDomain == domain && c.IsActive);
            if (conn == null) return new SsoLookupResultDto { SsoAvailable = false };
            return new SsoLookupResultDto { SsoAvailable = true, SsoConnectionId = conn.Id, Protocol = conn.Protocol, IdpSsoUrl = conn.IdpSsoUrl };
        }

        private async Task<SsoConnectionDto> ToDtoAsync(SsoConnection c)
        {
            var account = c.CorporateAccount ?? await _db.CorporateAccounts.FindAsync(c.CorporateAccountId);
            return new SsoConnectionDto { Id = c.Id, CorporateAccountId = c.CorporateAccountId, CompanyName = account?.CompanyName, Protocol = c.Protocol, EmailDomain = c.EmailDomain, IsActive = c.IsActive, CreatedAt = c.CreatedAt };
        }
    }

    // ── M138: Enterprise — Enterprise Audit Log ─────────────────────
    public class EnterpriseAuditService
    {
        private readonly AppDbContext _db;
        public EnterpriseAuditService(AppDbContext db) => _db = db;

        public async Task LogAsync(LogEnterpriseAuditRequestDto req)
        {
            _db.EnterpriseAuditEntries.Add(new EnterpriseAuditEntry
            {
                CorporateAccountId = req.CorporateAccountId, Action = req.Action, ActorUserId = req.ActorUserId, Details = req.Details, IpAddress = req.IpAddress,
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<EnterpriseAuditEntryDto>> GetForAccountAsync(int corporateAccountId, int take = 200)
        {
            var entries = await _db.EnterpriseAuditEntries.Include(e => e.ActorUser).Where(e => e.CorporateAccountId == corporateAccountId).OrderByDescending(e => e.OccurredAt).Take(take).ToListAsync();
            return entries.Select(e => new EnterpriseAuditEntryDto { Action = e.Action, ActorName = e.ActorUser?.FullName, Details = e.Details, OccurredAt = e.OccurredAt }).ToList();
        }
    }

    // ── M139: Chef Business Suite — Inventory Management ────────────
    public class ChefInventoryService
    {
        private readonly AppDbContext _db;
        public ChefInventoryService(AppDbContext db) => _db = db;

        public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemRequestDto req)
        {
            var item = new InventoryItem { ChefId = req.ChefId, IngredientName = req.IngredientName, Unit = req.Unit, ReorderThreshold = req.ReorderThreshold };
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return ToDto(item);
        }

        public async Task<List<InventoryItemDto>> GetForChefAsync(int chefId, bool lowStockOnly = false)
        {
            var items = await _db.InventoryItems.Where(i => i.ChefId == chefId).ToListAsync();
            var dtos = items.Select(ToDto);
            if (lowStockOnly) dtos = dtos.Where(d => d.IsLowStock);
            return dtos.ToList();
        }

        // Applies a transaction and updates the running QuantityOnHand —
        // the single write path for stock changes, so QuantityOnHand always
        // reflects the sum of its transaction history.
        public async Task<(bool Success, string Message, InventoryItemDto? Item)> RecordTransactionAsync(RecordInventoryTransactionRequestDto req)
        {
            var item = await _db.InventoryItems.FindAsync(req.InventoryItemId);
            if (item == null) return (false, "Inventory item not found.", null);

            var newQty = item.QuantityOnHand + req.QuantityChange;
            if (newQty < 0) return (false, "This would take quantity on hand below zero.", null);

            _db.InventoryTransactions.Add(new InventoryTransaction { InventoryItemId = req.InventoryItemId, ChangeType = req.ChangeType, QuantityChange = req.QuantityChange, Reason = req.Reason });
            item.QuantityOnHand = newQty;
            if (req.ChangeType == "Restock") item.LastRestockedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Recorded.", ToDto(item));
        }

        public async Task<List<InventoryTransactionDto>> GetHistoryAsync(int inventoryItemId)
            => (await _db.InventoryTransactions.Where(t => t.InventoryItemId == inventoryItemId).OrderByDescending(t => t.RecordedAt).ToListAsync())
                .Select(t => new InventoryTransactionDto { ChangeType = t.ChangeType, QuantityChange = t.QuantityChange, Reason = t.Reason, RecordedAt = t.RecordedAt }).ToList();

        private static InventoryItemDto ToDto(InventoryItem i) => new()
        {
            Id = i.Id, IngredientName = i.IngredientName, Unit = i.Unit, QuantityOnHand = i.QuantityOnHand,
            ReorderThreshold = i.ReorderThreshold, IsLowStock = i.QuantityOnHand <= i.ReorderThreshold, LastRestockedAt = i.LastRestockedAt,
        };
    }

    // ── M140: Chef Business Suite — Recipe Costing ──────────────────
    public class RecipeCostingService
    {
        private readonly AppDbContext _db;
        public RecipeCostingService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, RecipeIngredientDto? Ingredient)> AddIngredientAsync(AddRecipeIngredientRequestDto req)
        {
            if (!await _db.ChefMenuItems.AnyAsync(m => m.Id == req.ChefMenuItemId)) return (false, "Menu item not found.", null);
            var ing = new RecipeIngredient { ChefMenuItemId = req.ChefMenuItemId, IngredientName = req.IngredientName, QuantityUsed = req.QuantityUsed, Unit = req.Unit, UnitCost = req.UnitCost };
            _db.RecipeIngredients.Add(ing);
            await _db.SaveChangesAsync();
            return (true, "Ingredient added.", ToDto(ing));
        }

        // Sums ingredient line costs against the menu item's listed Price to
        // show the chef their real margin, then suggests a price for a
        // target margin using the standard cost-plus formula:
        // price = cost / (1 - targetMargin%).
        public async Task<(bool Success, string Message, RecipeCostBreakdownDto? Breakdown)> GetBreakdownAsync(RecipeCostBreakdownRequestDto req)
        {
            var menuItem = await _db.ChefMenuItems.FindAsync(req.ChefMenuItemId);
            if (menuItem == null) return (false, "Menu item not found.", null);

            var ingredients = await _db.RecipeIngredients.Where(i => i.ChefMenuItemId == req.ChefMenuItemId).ToListAsync();
            var totalCost = ingredients.Sum(i => i.QuantityUsed * i.UnitCost);
            var currentMargin = menuItem.Price == 0 ? 0 : Math.Round((menuItem.Price - totalCost) / menuItem.Price * 100, 1);
            var targetFraction = Math.Clamp(req.TargetMarginPercent, 0, 95) / 100;
            var suggestedPrice = targetFraction >= 1 ? totalCost : Math.Round(totalCost / (1 - targetFraction), 2);

            return (true, "OK", new RecipeCostBreakdownDto
            {
                ChefMenuItemId = req.ChefMenuItemId, DishName = menuItem.DishName, CurrentPrice = menuItem.Price, TotalIngredientCost = totalCost,
                CurrentMarginPercent = currentMargin, SuggestedPriceAtTargetMargin = suggestedPrice, Ingredients = ingredients.Select(ToDto).ToList(),
            });
        }

        public async Task<(bool Success, string Message)> RemoveIngredientAsync(int ingredientId)
        {
            var ing = await _db.RecipeIngredients.FindAsync(ingredientId);
            if (ing == null) return (false, "Ingredient not found.");
            _db.RecipeIngredients.Remove(ing);
            await _db.SaveChangesAsync();
            return (true, "Removed.");
        }

        private static RecipeIngredientDto ToDto(RecipeIngredient i) => new()
        {
            Id = i.Id, IngredientName = i.IngredientName, QuantityUsed = i.QuantityUsed, Unit = i.Unit, UnitCost = i.UnitCost, LineCost = Math.Round(i.QuantityUsed * i.UnitCost, 2),
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/sso-connections"), Authorize(Roles = "Admin")]
    public class SsoConnectionController : ControllerBase
    {
        private readonly Services.SsoConnectionService _svc;
        public SsoConnectionController(Services.SsoConnectionService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSsoConnectionRequestDto req)
        {
            var (success, message, conn) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = conn });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("activate")]
        public async Task<IActionResult> SetActive([FromBody] ActivateSsoConnectionRequestDto req)
        {
            var (success, message) = await _svc.SetActiveAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/sso-connections/lookup"), AllowAnonymous]
    public class SsoLookupController : ControllerBase
    {
        private readonly Services.SsoConnectionService _svc;
        public SsoLookupController(Services.SsoConnectionService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Lookup([FromBody] LookupSsoByDomainRequestDto req) => Ok(new { success = true, data = await _svc.LookupByEmailAsync(req.Email) });
    }

    [ApiController, Route("api/enterprise-audit"), Authorize(Roles = "Admin")]
    public class EnterpriseAuditController : ControllerBase
    {
        private readonly Services.EnterpriseAuditService _svc;
        public EnterpriseAuditController(Services.EnterpriseAuditService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Log([FromBody] LogEnterpriseAuditRequestDto req) { await _svc.LogAsync(req); return Ok(new { success = true }); }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetForAccount(int accountId, [FromQuery] int take = 200) => Ok(new { success = true, data = await _svc.GetForAccountAsync(accountId, take) });
    }

    [ApiController, Route("api/chef-inventory")]
    public class ChefInventoryController : ControllerBase
    {
        private readonly Services.ChefInventoryService _svc;
        public ChefInventoryController(Services.ChefInventoryService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("chef/{chefId}"), Authorize]
        public async Task<IActionResult> GetForChef(int chefId, [FromQuery] bool lowStockOnly = false) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId, lowStockOnly) });

        [HttpPost("transactions"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> RecordTransaction([FromBody] RecordInventoryTransactionRequestDto req)
        {
            var (success, message, item) = await _svc.RecordTransactionAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = item });
        }

        [HttpGet("{itemId}/history"), Authorize]
        public async Task<IActionResult> GetHistory(int itemId) => Ok(new { success = true, data = await _svc.GetHistoryAsync(itemId) });
    }

    [ApiController, Route("api/recipe-costing")]
    public class RecipeCostingController : ControllerBase
    {
        private readonly Services.RecipeCostingService _svc;
        public RecipeCostingController(Services.RecipeCostingService svc) => _svc = svc;

        [HttpPost("ingredients"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> AddIngredient([FromBody] AddRecipeIngredientRequestDto req)
        {
            var (success, message, ing) = await _svc.AddIngredientAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = ing });
        }

        [HttpPost("breakdown"), Authorize]
        public async Task<IActionResult> GetBreakdown([FromBody] RecipeCostBreakdownRequestDto req)
        {
            var (success, message, breakdown) = await _svc.GetBreakdownAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data = breakdown });
        }

        [HttpDelete("ingredients/{id}"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> RemoveIngredient(int id)
        {
            var (success, message) = await _svc.RemoveIngredientAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
