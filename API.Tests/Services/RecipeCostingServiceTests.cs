using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using LovEat.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LovEat.API.Tests.Services
{
    public class RecipeCostingServiceTests
    {
        private static AppDbContext NewInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static async Task<int> SeedMenuItemAsync(AppDbContext db, decimal price)
        {
            var item = new ChefMenuItem { ChefProfileId = 1, DishName = "Chicken Biryani", Price = price };
            db.ChefMenuItems.Add(item);
            await db.SaveChangesAsync();
            return item.Id;
        }

        [Fact]
        public async Task GetBreakdownAsync_ComputesTotalIngredientCost_AsSumOfLineCosts()
        {
            await using var db = NewInMemoryDb();
            var itemId = await SeedMenuItemAsync(db, price: 250m);
            var svc = new RecipeCostingService(db);

            await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = itemId, IngredientName = "Basmati Rice", QuantityUsed = 200, Unit = "g", UnitCost = 0.15m });
            await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = itemId, IngredientName = "Chicken", QuantityUsed = 150, Unit = "g", UnitCost = 0.4m });

            var (success, _, breakdown) = await svc.GetBreakdownAsync(new RecipeCostBreakdownRequestDto { ChefMenuItemId = itemId, TargetMarginPercent = 40 });

            Assert.True(success);
            // 200*0.15 + 150*0.4 = 30 + 60 = 90
            Assert.Equal(90m, breakdown!.TotalIngredientCost);
        }

        [Fact]
        public async Task GetBreakdownAsync_ComputesCurrentMargin_Correctly()
        {
            await using var db = NewInMemoryDb();
            var itemId = await SeedMenuItemAsync(db, price: 200m);
            var svc = new RecipeCostingService(db);
            await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = itemId, IngredientName = "X", QuantityUsed = 1, Unit = "kg", UnitCost = 100m });

            var (_, _, breakdown) = await svc.GetBreakdownAsync(new RecipeCostBreakdownRequestDto { ChefMenuItemId = itemId, TargetMarginPercent = 40 });

            // margin = (price - cost) / price * 100 = (200-100)/200*100 = 50%
            Assert.Equal(50m, breakdown!.CurrentMarginPercent);
        }

        [Fact]
        public async Task GetBreakdownAsync_SuggestsHigherPrice_ForCostPlusTargetMargin()
        {
            await using var db = NewInMemoryDb();
            var itemId = await SeedMenuItemAsync(db, price: 100m);
            var svc = new RecipeCostingService(db);
            await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = itemId, IngredientName = "X", QuantityUsed = 1, Unit = "kg", UnitCost = 60m });

            var (_, _, breakdown) = await svc.GetBreakdownAsync(new RecipeCostBreakdownRequestDto { ChefMenuItemId = itemId, TargetMarginPercent = 40 });

            // suggestedPrice = cost / (1 - targetMargin%) = 60 / (1 - 0.4) = 100
            Assert.Equal(100m, breakdown!.SuggestedPriceAtTargetMargin);
        }

        [Fact]
        public async Task GetBreakdownAsync_Fails_WhenMenuItemDoesNotExist()
        {
            await using var db = NewInMemoryDb();
            var svc = new RecipeCostingService(db);

            var (success, message, breakdown) = await svc.GetBreakdownAsync(new RecipeCostBreakdownRequestDto { ChefMenuItemId = 9999, TargetMarginPercent = 40 });

            Assert.False(success);
            Assert.Null(breakdown);
        }

        [Fact]
        public async Task AddIngredientAsync_Fails_WhenMenuItemDoesNotExist()
        {
            await using var db = NewInMemoryDb();
            var svc = new RecipeCostingService(db);

            var (success, message, ingredient) = await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = 9999, IngredientName = "X", QuantityUsed = 1, Unit = "kg", UnitCost = 10 });

            Assert.False(success);
            Assert.Null(ingredient);
        }

        [Fact]
        public async Task RemoveIngredientAsync_RemovesIt_AndExcludesItFromSubsequentBreakdown()
        {
            await using var db = NewInMemoryDb();
            var itemId = await SeedMenuItemAsync(db, price: 200m);
            var svc = new RecipeCostingService(db);
            var (_, _, ingredient) = await svc.AddIngredientAsync(new AddRecipeIngredientRequestDto { ChefMenuItemId = itemId, IngredientName = "X", QuantityUsed = 1, Unit = "kg", UnitCost = 50m });

            await svc.RemoveIngredientAsync(ingredient!.Id);
            var (_, _, breakdown) = await svc.GetBreakdownAsync(new RecipeCostBreakdownRequestDto { ChefMenuItemId = itemId, TargetMarginPercent = 40 });

            Assert.Equal(0m, breakdown!.TotalIngredientCost);
            Assert.Empty(breakdown.Ingredients);
        }
    }
}
