using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;

namespace LovEat.API.Tests.TestSupport
{
    /// <summary>
    /// Every test gets a brand-new EF Core InMemory database, named with a fresh Guid,
    /// so tests never see each other's data and can run in parallel safely.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
