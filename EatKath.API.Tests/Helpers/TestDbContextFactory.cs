using EatKath.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EatKath.API.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext Create()
        {
            // The InMemory provider does not implement real transactions
            // (Phase 5O introduced Database.BeginTransactionAsync in
            // ReservationService.CreateAsync). By default EF Core raises
            // TransactionIgnoredWarning as an error for this. Suppressing
            // it is safe for tests: InMemory simply ignores the
            // transaction and executes each SaveChangesAsync directly -
            // this exercises all business logic and query behavior
            // correctly, but it means InMemory tests cannot verify real
            // commit/rollback atomicity, isolation-level locking, or the
            // filtered unique index - those require a real SQL Server.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}