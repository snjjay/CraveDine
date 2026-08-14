using EatKath.API.Entities; //Your Entities are your database models: Area, Restaurant, User Deal, MenuItem
using Microsoft.EntityFrameworkCore; //Bring in Entity Framework and my database entities which gives u things like  DbContext DbSet ModelBuilder




//Think of it as the bridge between your Service and SQL Server.
//The Service doesn't directly talk to SQL Server. It talks through ApplicationDbContext.
namespace EatKath.API.Data
{
    public class ApplicationDbContext : DbContext //ApplicationDbContext = EatKath's connection/bridge to the database.
    {

        //Constructor — DI again
        //Remember following in Program.cs
        //builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //options.UseSqlServer(
        //builder.Configuration.GetConnectionString("DefaultConnection")));
        //That tells .NET: >Create ApplicationDbContext and use SQL Server with this connection string
        //Program.cs configures the DbContext; DbContext talks to the database.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) //Constructor — DI again
            : base(options)
        {
        }

        // Lookup Tables
        //DbSet — Your database tables
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Area> Areas => Set<Area>(); //There is an Areas table containing Area records. So Area Entity>DbSet<Area> > Areas table.
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

        //OnModelCreating — Define how tables relate: Tell Entity Framework exactly how my database tables should be designed and related. How are those tables related and what rules do they have?
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Composite Keys This means the table uses two columns together as the primary key.
            // ============================

            modelBuilder.Entity<RestaurantCuisine>()
                .HasKey(x => new { x.RestaurantId, x.CuisineId });

            modelBuilder.Entity<RestaurantDiningType>()
                .HasKey(x => new { x.RestaurantId, x.DiningTypeId });

            modelBuilder.Entity<UserFavorite>()
                .HasKey(x => new { x.UserId, x.RestaurantId });

            // ============================
            // User→ Role relationship> e.g One Role can have many Users. RoleId in User is the foreign key.
            // ============================

            modelBuilder.Entity<User>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // Restaurant → Owner>One owner can have many restaurants.
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
            // Restaurant Images:One Restaurant can have many Images.
            // ============================

            modelBuilder.Entity<RestaurantImage>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade); //Cascade :Restaurant deleted>Its images are automatically deleted

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
            //This is your joining table: Restaurant>RestaurantCuisine>Cuisine. This allows One restaurant → many cuisines and One cuisine → many restaurants. That's a many-to-many relationship.
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
            // Menu: One Restaurant has many MenuItems
            // ============================

            modelBuilder.Entity<MenuItem>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
            
            //One MenuCategory has many MenuItems.
            modelBuilder.Entity<MenuItem>()
                .HasOne(x => x.MenuCategory)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.MenuCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Deals One restaurant can have many deals.
            // ============================

            modelBuilder.Entity<Deal>()
                .HasOne(x => x.Restaurant)
                .WithMany(x => x.Deals)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // Redemptions
            // ============================
            //One User can have many Redemptions.
            modelBuilder.Entity<Redemption>()
                .HasOne(x => x.User)
                .WithMany(x => x.Redemptions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            //One Deal can have many Redemptions.
            modelBuilder.Entity<Redemption>()
                .HasOne(x => x.Deal)
                .WithMany(x => x.Redemptions)
                .HasForeignKey(x => x.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // User Favorites: many-to-many relationship.
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
                .HasPrecision(18, 2); //This is a monetary/decimal value. Store it with 2 decimal places. e.g 25.50


            // ============================
            // Reservations One User can have many Reservations.
            // ============================

            modelBuilder.Entity<Reservation>()
                .HasOne(x => x.User)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(x => x.Deal)
                .WithMany()
                .HasForeignKey(x => x.DealId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .IsUnique(); //Area names must be unique in the database




        }
    }
}

// ==========================================================
// APPLICATION DB CONTEXT
// ==========================================================
//
// 🗄️ Think: "The bridge between my application and database."
//
// Service
//    ↓
// ApplicationDbContext
//    ↓
// SQL Server
//
// ----------------------------------------------------------
//
// DbSet = represents a database table.
//
// DbSet<Area>         → Areas table
// DbSet<User>         → Users table
// DbSet<Restaurant>   → Restaurants table
// DbSet<Deal>         → Deals table
// etc.
//
// ----------------------------------------------------------
//
// OnModelCreating
//
// 🏗️ Think: "Tell EF how my database is designed."
//
// It defines:
// - Relationships between tables
// - Foreign keys
// - Composite keys
// - Delete behavior
// - Decimal precision
// - Unique indexes
//
// ----------------------------------------------------------
//
// 🔑 Remember:
//
// DbSet            = WHAT tables do I have?
// OnModelCreating  = HOW are those tables related/rules?
// DbContext        = BRIDGE between Service and Database
//
// ==========================================================




// ==========================================================
// WHERE DOES ApplicationDbContext FIT?
// ==========================================================
//
// 📱 React
//    ↓
// 📦 DTO
//    ↓
// 🛂 Validator
//    ↓
// 🎯 Controller
//    ↓
// ⚙️ Service
//    ↓
// 🗄️ ApplicationDbContext  ← YOU ARE HERE
//    ↓
// 🗄️ SQL Server Database
//
// ApplicationDbContext is the BRIDGE between the Service
// and the actual database.
//
// Service says:
// "I need Areas from the database."
//        ↓
// ApplicationDbContext
//        ↓
// "I'll get them from SQL Server."
//