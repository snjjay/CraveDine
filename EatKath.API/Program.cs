using Azure.Core;
using EatKath.API.Data;
using EatKath.API.Data.Seeders;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Mappings;
using EatKath.API.Middleware;
using EatKath.API.Services;
using EatKath.API.Services.Interfaces;
using EatKath.API.Validators.Area;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("================================");
Console.WriteLine("App is starting...");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Connection string exists: {!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"))}");
Console.WriteLine("================================");

// ==========================================================
// Add Services
// ==========================================================

builder.Services.AddControllers();

// ==========================================================
// CORS:"Who is allowed to talk to CraveDine API?"
// ==========================================================
//
// Allow the React application (Vite) running on
// http://localhost:5173
// to access this Web API.
//
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://yellow-ocean-0fc06c300.7.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ==========================================================
// Database:"Where is my database?
// ==========================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ==========================================================
// Dependency Injection (DI)
// ==========================================================
//
// Think: "I need a key ? someone gives me the key."
//
// I don't make the key myself.
// I simply ask for what I need, and .NET gives it to me.
//
// Example:
// IDealService = "I need a DealService"
// DealService  = "Here is the DealService"
//
// Program.cs = the place where we tell .NET what to give me.
// ==========================================================
builder.Services.AddScoped<IAreaService, AreaService>();  //AddScoped<INTERFACE, REAL CLASS>();  "If somebody asks for LEFT, give them RIGHT."
builder.Services.AddScoped<ICuisineService, CuisineService>();
builder.Services.AddScoped<IDiningTypeService, DiningTypeService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IRestaurantImageService, RestaurantImageService>();
builder.Services.AddScoped<IRestaurantOpeningHourService, RestaurantOpeningHourService>();
builder.Services.AddScoped<IUserFavoriteService, UserFavoriteService>();
builder.Services.AddScoped<IRedemptionService, RedemptionService>();
builder.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<FileStorageService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));  //"Enable automatic conversion between my DTOs and entities.  //Deal Entity> AutoMapper> DealDto

builder.Services.AddHttpContextAccessor(); //"Allow my services to find information about the current HTTP request/user."// Useful when CurrentUserService needs to know: "Who is currently logged in?"

builder.Services.AddValidatorsFromAssemblyContaining<CreateAreaValidator>(); // "Find and register my validation rules. // Create Area request.>Validator>Is this data valid?

builder.Services.AddScoped<IReservationService, ReservationService>();

// ==========================================================
// Swagger: Give me a screen where I can see and test my APIs
// ==========================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================================
// JWT Authentication: How do I know who you are?
// User logs> JWT token> React stores token>React calls API with token>API checks token>"Yes, this user is authenticated."
// ==========================================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// ==========================================================
// Build Application: Okay, I've given you all my instructions. Now build my application
// ==========================================================

var app = builder.Build();

// ==========================================================
// Configure Middleware :What happens to every request?

//app.UseSwagger();
//app.UseHttpsRedirection();
//app.UseCors("ReactPolicy");
//app.UseAuthentication();
//app.UseAuthorization();
//app.UseMiddleware<ExceptionMiddleware>();
//app.UseStaticFiles();
//app.MapControllers();

//Imagine a request coming into EatKath: REQUEST>HTTPS check> CORS>Authentication>Authorization>Exception handling>Controller>RESPONSE                   
//Each middleware gets a chance to do something.
// ==========================================================

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

// Always redirect HTTP -> HTTPS
app.UseHttpsRedirection(); //Use HTTPS, not insecure HTTP.

// Always allow React frontend
app.UseCors("ReactPolicy"); //Apply my CORS door rules.

app.UseAuthentication(); //Check who this user is

app.UseAuthorization(); // Check whether this user is allowed to do this.

app.UseMiddleware<ExceptionMiddleware>(); //If something crashes, handle the error properly

app.UseStaticFiles(); //Allow the application to serve files such as uploaded images

app.MapControllers(); // Now connect incoming API URLs to my controllers POST /api/deals to Deal Controller

// ==========================================================
// Seed Database:make sure the database has the initial data it needs
// ==========================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 10;

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            Console.WriteLine($"Database seed attempt {i}/{maxRetries}...");

            await DatabaseSeeder.SeedAsync(context);

            Console.WriteLine("Database seeded successfully.");

            break;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Console.WriteLine($"SQL Server not ready (Attempt {i}/{maxRetries})");
            Console.WriteLine(ex);

            if (i == maxRetries)
                throw;

            await Task.Delay(TimeSpan.FromSeconds(5)); //Maybe SQL Server isn't ready yet. Wait 5 seconds and try again
        }
        catch (Exception ex)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("APPLICATION STARTUP ERROR");
            Console.WriteLine(ex);
            Console.WriteLine("========================================");

            throw;
        }
    }
}

Console.WriteLine($"ContentRoot: {app.Environment.ContentRootPath}");
Console.WriteLine($"WebRoot: {app.Environment.WebRootPath}");
app.Run(); //Start the web application and begin accepting requests



// ==========================================================
//              EATKATH - PROGRAM.CS ROADMAP
// ==========================================================
//
// PROGRAM.CS
// ?
// ??? 1. Imports
// ?      "What tools do I need?"
// ?
// ??? 2. Builder
// ?      "Start setting up EatKath"
// ?
// ??? 3. Controllers
// ?      "I have API controllers"
// ?
// ??? 4. CORS
// ?      "Who can talk to my API?"
// ?
// ??? 5. Database
// ?      "Where is my database?"
// ?
// ??? 6. Dependency Injection
// ?      "If something needs something, give it to them"
// ?
// ??? 7. Swagger
// ?      "Give me an API testing screen"
// ?
// ??? 8. Authentication
// ?      "Who are you?"
// ?
// ??? 9. Authorization
// ?      "Are you allowed?"
// ?
// ??? 10. Build
// ?       "Okay, build the application"
// ?
// ??? 11. Middleware
// ?       "What happens to every request?"
// ?
// ??? 12. Database Seed
// ?       "Put initial data into the database"
// ?
// ??? 13. app.Run()
//         "START EATKATH ??"
//
// ==========================================================
// ==========================================================