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
        public DbSet<MenuItem> Menus => Set<MenuItem>();
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        // Business Tables
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Redemption> Redemptions => Set<Redemption>();
        public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Composite Primary Keys
            // ============================

            modelBuilder.Entity<RestaurantCuisine>()
                .HasKey(rc => new { rc.RestaurantId, rc.CuisineId });

            modelBuilder.Entity<RestaurantDiningType>()
                .HasKey(rdt => new { rdt.RestaurantId, rdt.DiningTypeId });

            modelBuilder.Entity<UserFavorite>()
                .HasKey(uf => new { uf.UserId, uf.RestaurantId });

            // ============================
            // User Relationships
            // ============================

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // Restaurant Relationships
            // ============================

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Owner)
                .WithMany(u => u.Restaurants)
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Area)
                .WithMany(a => a.Restaurants)
                .HasForeignKey(r => r.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // Restaurant Images
            // ============================

            modelBuilder.Entity<RestaurantImage>()
                .HasOne(ri => ri.Restaurant)
                .WithMany(r => r.Images)
                .HasForeignKey(ri => ri.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Restaurant Opening Hours
            // ============================

            modelBuilder.Entity<RestaurantOpeningHour>()
                .HasOne(roh => roh.Restaurant)
                .WithMany(r => r.OpeningHours)
                .HasForeignKey(roh => roh.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Restaurant Cuisine
            // ============================

            modelBuilder.Entity<RestaurantCuisine>()
                .HasOne(rc => rc.Restaurant)
                .WithMany(r => r.RestaurantCuisines)
                .HasForeignKey(rc => rc.RestaurantId);

            modelBuilder.Entity<RestaurantCuisine>()
                .HasOne(rc => rc.Cuisine)
                .WithMany(c => c.RestaurantCuisines)
                .HasForeignKey(rc => rc.CuisineId);

            // ============================
            // Restaurant Dining Types
            // ============================

            modelBuilder.Entity<RestaurantDiningType>()
                .HasOne(rdt => rdt.Restaurant)
                .WithMany(r => r.RestaurantDiningTypes)
                .HasForeignKey(rdt => rdt.RestaurantId);

            modelBuilder.Entity<RestaurantDiningType>()
                .HasOne(rdt => rdt.DiningType)
                .WithMany(dt => dt.RestaurantDiningTypes)
                .HasForeignKey(rdt => rdt.DiningTypeId);

            // ============================
            // Menu Relationships
            // ============================

                modelBuilder.Entity<MenuItem>()
           .HasOne(m => m.Restaurant)
           .WithMany(r => r.MenuItems)
           .HasForeignKey(m => m.RestaurantId)
           .OnDelete(DeleteBehavior.Cascade);

            

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.MenuCategory)
                .WithMany(mc => mc.MenuItems)
                .HasForeignKey(mi => mi.MenuCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Deals
            // ============================

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Restaurant)
                .WithMany(r => r.Deals)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Redemptions
            // ============================

            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.User)
                .WithMany(u => u.Redemptions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Redemption>()
                .HasOne(r => r.Deal)
                .WithMany(d => d.Redemptions)
                .HasForeignKey(r => r.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // User Favorites
            // ============================

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.UserFavorites)
                .HasForeignKey(uf => uf.UserId);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Restaurant)
                .WithMany(r => r.UserFavorites)
                .HasForeignKey(uf => uf.RestaurantId);

            // ============================
            // Unique Constraints
            // ============================

            modelBuilder.Entity<Area>()
                .HasIndex(a => a.Name)
                .IsUnique();



        }
    }
}