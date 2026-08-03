using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Lookup Tables
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Area> Areas => Set<Area>();
        public DbSet<Cuisine> Cuisines => Set<Cuisine>();
        public DbSet<DiningType> DiningTypes => Set<DiningType>();

        // Core Tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();

        // Restaurant Tables
        public DbSet<RestaurantImage> RestaurantImages => Set<RestaurantImage>();
        public DbSet<RestaurantOpeningHour> RestaurantOpeningHours => Set<RestaurantOpeningHour>();
        public DbSet<RestaurantCuisine> RestaurantCuisines => Set<RestaurantCuisine>();
        public DbSet<RestaurantDiningType> RestaurantDiningTypes => Set<RestaurantDiningType>();

        // Menu Tables
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        // Business Tables
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Redemption> Redemptions => Set<Redemption>();
        public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

        public DbSet<Reservation> Reservations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Composite Keys
            // ============================

            modelBuilder.Entity<RestaurantCuisine>()
                .HasKey(x => new { x.RestaurantId, x.CuisineId });

            modelBuilder.Entity<RestaurantDiningType>()
                .HasKey(x => new { x.RestaurantId, x.DiningTypeId });

            modelBuilder.Entity<UserFavorite>()
                .HasKey(x => new { x.UserId, x.RestaurantId });

            // ============================
            // User
            // ============================

            modelBuilder.Entity<User>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // Restaurant
            // ============================

            modelBuilder.Entity<Restaurant>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Restaurants)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Restaurant>()
                .HasOne(x => x.Area)
                .WithMany(x => x.Restaurants)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // Restaurant Images
            // ============================

            modelBuilder.Entity<RestaurantImage>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Restaurant Opening Hours
            // ============================

            modelBuilder.Entity<RestaurantOpeningHour>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.OpeningHours)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Restaurant Cuisine
            // ============================

            modelBuilder.Entity<RestaurantCuisine>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.RestaurantCuisines)
                .HasForeignKey(x => x.RestaurantId);

            modelBuilder.Entity<RestaurantCuisine>()
                .HasOne(x => x.Cuisine)
                .WithMany(x => x.RestaurantCuisines)
                .HasForeignKey(x => x.CuisineId);

            // ============================
            // Restaurant Dining Type
            // ============================

            modelBuilder.Entity<RestaurantDiningType>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.RestaurantDiningTypes)
                .HasForeignKey(x => x.RestaurantId);

            modelBuilder.Entity<RestaurantDiningType>()
                .HasOne(x => x.DiningType)
                .WithMany(x => x.RestaurantDiningTypes)
                .HasForeignKey(x => x.DiningTypeId);

            // ============================
            // Menu
            // ============================

            modelBuilder.Entity<MenuItem>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MenuItem>()
                .HasOne(x => x.MenuCategory)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.MenuCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Deals
            // ============================

            modelBuilder.Entity<Deal>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.Deals)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Redemptions
            // ============================

            modelBuilder.Entity<Redemption>()
                .HasOne(x => x.User)
                .WithMany(x => x.Redemptions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Redemption>()
                .HasOne(x => x.Deal)
                .WithMany(x => x.Redemptions)
                .HasForeignKey(x => x.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // User Favorites
            // ============================

            modelBuilder.Entity<UserFavorite>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserFavorites)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.UserFavorites)
                .HasForeignKey(x => x.RestaurantId);

            // ============================
            // Decimal Precision
            // ============================
            //
            // Explicitly define SQL Server precision
            // for decimal columns to avoid EF Core warnings
            // and ensure monetary values are stored correctly.
            //
            // decimal(18,2)
            // - 18 total digits
            // - 2 decimal places
            //

            modelBuilder.Entity<Deal>()
                .Property(x => x.DiscountPercentage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MenuItem>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Redemption>()
                .Property(x => x.BillAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Redemption>()
                .Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Redemption>()
                .Property(x => x.FinalAmount)
                .HasPrecision(18, 2);

            // ============================
            // Unique Indexes
            // ============================

            modelBuilder.Entity<Area>()
                .HasIndex(x => x.Name)
                .IsUnique();
        }
    }
}