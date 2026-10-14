using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Migrations;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Data;

// Surprise Bags data model (Step 1). Metadata is read from a SQL Server
// configured context - the real provider's model - without ever connecting
// to a database. InMemory cannot enforce check constraints or filtered
// indexes, so those are verified here as configuration and generated SQL.
[TestClass]
public class SurpriseBagModelTests
{
    private static ApplicationDbContext CreateSqlServerContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\ModelOnly;Database=ModelOnly;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options);
    }

    // The full model, including relational annotations (filters, check
    // constraints) that the read-optimised runtime model leaves out.
    private static IModel DesignTimeModel(ApplicationDbContext context) =>
        context.GetService<IDesignTimeModel>().Model;

    [TestMethod]
    public void Relationships_ShouldUseApprovedDeleteBehaviours()
    {
        using var context = CreateSqlServerContext();
        var model = DesignTimeModel(context);

        var bag = model.FindEntityType(typeof(SurpriseBag))!;
        var reservation = model.FindEntityType(typeof(SurpriseBagReservation))!;

        var bagToRestaurant = bag.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Restaurant));
        bagToRestaurant.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        bagToRestaurant.PrincipalToDependent.Should().BeNull("Restaurant.cs is unchanged (one-way navigation)");

        var reservationToBag = reservation.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(SurpriseBag));
        reservationToBag.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        reservationToBag.PrincipalToDependent!.Name.Should().Be(nameof(SurpriseBag.Reservations));

        var reservationToUser = reservation.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(User));
        reservationToUser.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        reservationToUser.PrincipalToDependent.Should().BeNull("User.cs is unchanged (one-way navigation)");
    }

    [TestMethod]
    [DataRow(typeof(SurpriseBag), nameof(SurpriseBag.Price))]
    [DataRow(typeof(SurpriseBag), nameof(SurpriseBag.EstimatedValue))]
    [DataRow(typeof(SurpriseBagReservation), nameof(SurpriseBagReservation.UnitPrice))]
    public void MoneyColumns_ShouldBeDecimal18_2(Type entity, string property)
    {
        using var context = CreateSqlServerContext();

        var column = DesignTimeModel(context).FindEntityType(entity)!.FindProperty(property)!;

        column.GetPrecision().Should().Be(18);
        column.GetScale().Should().Be(2);
        column.GetColumnType().Should().Be("decimal(18,2)");
    }

    [TestMethod]
    [DataRow(typeof(SurpriseBag), nameof(SurpriseBag.Title), 150)]
    [DataRow(typeof(SurpriseBag), nameof(SurpriseBag.Description), 1000)]
    [DataRow(typeof(SurpriseBag), nameof(SurpriseBag.Conditions), 1000)]
    [DataRow(typeof(SurpriseBagReservation), nameof(SurpriseBagReservation.ConfirmationCode), 12)]
    public void TextColumns_ShouldHaveApprovedMaxLengths(Type entity, string property, int maxLength)
    {
        using var context = CreateSqlServerContext();

        var column = DesignTimeModel(context).FindEntityType(entity)!.FindProperty(property)!;

        column.GetMaxLength().Should().Be(maxLength);
        column.IsNullable.Should().BeFalse();
    }

    // Optional bag image: a bounded, nullable path.
    [TestMethod]
    public void ImageUrl_ShouldBeOptionalNvarchar500()
    {
        using var context = CreateSqlServerContext();

        var column = DesignTimeModel(context).FindEntityType(typeof(SurpriseBag))!
            .FindProperty(nameof(SurpriseBag.ImageUrl))!;

        column.IsNullable.Should().BeTrue();
        column.GetMaxLength().Should().Be(500);
        column.GetColumnType().Should().Be("nvarchar(500)");
    }

    [TestMethod]
    public void ConfirmationCode_ShouldHaveUniqueIndex()
    {
        using var context = CreateSqlServerContext();
        var reservation = DesignTimeModel(context).FindEntityType(typeof(SurpriseBagReservation))!;

        var index = reservation.GetIndexes().Single(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(SurpriseBagReservation.ConfirmationCode) }));

        index.IsUnique.Should().BeTrue();
        index.GetFilter().Should().BeNull();
    }

    [TestMethod]
    public void ActiveReservationIndex_ShouldBeUniqueAndFilteredToReserved()
    {
        using var context = CreateSqlServerContext();
        var reservation = DesignTimeModel(context).FindEntityType(typeof(SurpriseBagReservation))!;

        var index = reservation.GetIndexes().Single(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(SurpriseBagReservation.SurpriseBagId),
                nameof(SurpriseBagReservation.UserId)
            }));

        index.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Be($"[Status] = {(int)SurpriseBagReservationStatus.Reserved}");
        index.GetFilter().Should().Be("[Status] = 1");
    }

    [TestMethod]
    public void CheckConstraints_ShouldMatchApprovedRules()
    {
        using var context = CreateSqlServerContext();
        var model = DesignTimeModel(context);

        model.FindEntityType(typeof(SurpriseBag))!.GetCheckConstraints()
            .ToDictionary(c => c.ModelName, c => c.Sql)
            .Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["CK_SurpriseBags_QuantityTotal"] = "[QuantityTotal] >= 1",
                ["CK_SurpriseBags_MaxPerCustomer"] = "[MaxPerCustomer] >= 1",
                ["CK_SurpriseBags_Price"] = "[Price] >= 0",
                ["CK_SurpriseBags_EstimatedValue"] = "[EstimatedValue] IS NULL OR [EstimatedValue] >= 0",
                ["CK_SurpriseBags_PickupWindow"] = "[PickupEndTime] > [PickupStartTime]"
            });

        model.FindEntityType(typeof(SurpriseBagReservation))!.GetCheckConstraints()
            .ToDictionary(c => c.ModelName, c => c.Sql)
            .Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["CK_SurpriseBagReservations_Quantity"] = "[Quantity] >= 1",
                ["CK_SurpriseBagReservations_UnitPrice"] = "[UnitPrice] >= 0",
                ["CK_SurpriseBagReservations_ConfirmationCode"] = "[ConfirmationCode] <> N''"
            });
    }

    // The SQL Server DDL EF generates from the model (no connection made).
    [TestMethod]
    public void SqlServerCreateScript_ShouldContainTablesConstraintsAndIndexes()
    {
        using var context = CreateSqlServerContext();

        var sql = context.Database.GenerateCreateScript();

        sql.Should().Contain("CREATE TABLE [SurpriseBags]");
        sql.Should().Contain("CREATE TABLE [SurpriseBagReservations]");
        sql.Should().Contain("[Price] decimal(18,2) NOT NULL");
        sql.Should().Contain("[EstimatedValue] decimal(18,2) NULL");
        sql.Should().Contain("[UnitPrice] decimal(18,2) NOT NULL");
        sql.Should().Contain("CONSTRAINT [CK_SurpriseBags_PickupWindow] CHECK ([PickupEndTime] > [PickupStartTime])");
        sql.Should().Contain("CONSTRAINT [CK_SurpriseBagReservations_ConfirmationCode] CHECK ([ConfirmationCode] <> N'')");
        sql.Should().Contain("CONSTRAINT [FK_SurpriseBags_Restaurants_RestaurantId] FOREIGN KEY ([RestaurantId]) REFERENCES [Restaurants] ([Id]) ON DELETE CASCADE");
        sql.Should().Contain("CONSTRAINT [FK_SurpriseBagReservations_SurpriseBags_SurpriseBagId] FOREIGN KEY ([SurpriseBagId]) REFERENCES [SurpriseBags] ([Id]) ON DELETE NO ACTION");
        sql.Should().Contain("CREATE UNIQUE INDEX [IX_SurpriseBagReservations_ConfirmationCode] ON [SurpriseBagReservations] ([ConfirmationCode]);");
        sql.Should().Contain("CREATE UNIQUE INDEX [IX_SurpriseBagReservations_SurpriseBagId_UserId] ON [SurpriseBagReservations] ([SurpriseBagId], [UserId]) WHERE [Status] = 1;");
    }

    [TestMethod]
    public async Task InMemory_RoundTrip_ShouldSaveAndLoadBagWithReservation()
    {
        var context = TestDbContextFactory.Create();

        var user = new User { FirstName = "Asha", LastName = "Shrestha", Email = "asha@example.com" };
        var restaurant = new Restaurant { Name = "Himalayan Bakery", IsActive = true, Owner = user };

        var bag = new SurpriseBag
        {
            Restaurant = restaurant,
            Title = "End-of-day bakery bag",
            Category = SurpriseBagCategory.Bakery,
            Price = 250.50m,
            EstimatedValue = 750m,
            QuantityTotal = 5,
            PickupDate = new DateOnly(2026, 11, 10),
            PickupStartTime = new TimeOnly(18, 0),
            PickupEndTime = new TimeOnly(19, 30)
        };

        context.SurpriseBags.Add(bag);
        context.SurpriseBagReservations.Add(new SurpriseBagReservation
        {
            SurpriseBag = bag,
            User = user,
            Quantity = 2,
            UnitPrice = 250.50m,
            ConfirmationCode = "K7M2Q9XA"
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var loaded = await context.SurpriseBags
            .Include(b => b.Reservations)
            .Include(b => b.Restaurant)
            .SingleAsync();

        loaded.Restaurant.Name.Should().Be("Himalayan Bakery");
        loaded.Category.Should().Be(SurpriseBagCategory.Bakery);
        loaded.Price.Should().Be(250.50m);
        loaded.EstimatedValue.Should().Be(750m);
        loaded.MaxPerCustomer.Should().Be(1);
        loaded.IsActive.Should().BeTrue();
        loaded.PickupDate.Should().Be(new DateOnly(2026, 11, 10));
        loaded.PickupEndTime.Should().Be(new TimeOnly(19, 30));

        var reservation = loaded.Reservations.Single();
        reservation.Status.Should().Be(SurpriseBagReservationStatus.Reserved);
        reservation.Quantity.Should().Be(2);
        reservation.UnitPrice.Should().Be(250.50m);
        reservation.ConfirmationCode.Should().Be("K7M2Q9XA");
        reservation.CollectedAt.Should().BeNull();
        reservation.CancelledAt.Should().BeNull();
    }

    // Guards the "additive only" promise: the migration may only create the
    // two new tables (and their indexes) and its Down() only drops them.
    [TestMethod]
    public void AddSurpriseBagsMigration_ShouldOnlyCreateAndDropTheTwoNewTables()
    {
        var newTables = new[] { "SurpriseBags", "SurpriseBagReservations" };
        var migration = new AddSurpriseBags();

        migration.UpOperations.Should().AllSatisfy(op =>
        {
            var table = op switch
            {
                CreateTableOperation create => create.Name,
                CreateIndexOperation index => index.Table,
                _ => throw new AssertFailedException($"Unexpected operation {op.GetType().Name}")
            };

            table.Should().BeOneOf(newTables);
        });

        migration.UpOperations.OfType<CreateTableOperation>().Select(t => t.Name)
            .Should().Equal("SurpriseBags", "SurpriseBagReservations");

        migration.DownOperations.Should().AllBeOfType<DropTableOperation>();
        migration.DownOperations.Cast<DropTableOperation>().Select(t => t.Name)
            .Should().Equal("SurpriseBagReservations", "SurpriseBags");
    }

    // Additive only: one nullable column, so existing bags (null = no
    // image) and their reservations are untouched; Down() drops just it.
    [TestMethod]
    public void AddSurpriseBagImageUrlMigration_ShouldOnlyAddAndDropOneNullableColumn()
    {
        var migration = new AddSurpriseBagImageUrl();

        var add = migration.UpOperations.Should().ContainSingle()
            .Which.Should().BeOfType<AddColumnOperation>().Subject;
        add.Table.Should().Be("SurpriseBags");
        add.Name.Should().Be("ImageUrl");
        add.IsNullable.Should().BeTrue();
        add.MaxLength.Should().Be(500);
        add.DefaultValue.Should().BeNull();

        var drop = migration.DownOperations.Should().ContainSingle()
            .Which.Should().BeOfType<DropColumnOperation>().Subject;
        drop.Table.Should().Be("SurpriseBags");
        drop.Name.Should().Be("ImageUrl");
    }
}
