using EatKath.API.DTOs.Area;

namespace EatKath.API.Interfaces;

public interface IAreaService //AreaService promises to follow everything defined in IAreaService
//Interface = a promise/list of what a class must be able to do.
//Any Area Service must be able to GET, CREATE, UPDATE and DELETE Areas
//The interface says WHAT must be available.
//The service contains HOW it is done.
//builder.Services.AddScoped<IAreaService, AreaService>(); //We saw Program.cs connects them
//This tells .net When somebody asks for the IAreaService promise, give them the AreaService class
{
    Task<IEnumerable<AreaDto>> GetAllAsync();

    Task<AreaDto?> GetByIdAsync(int id);

    Task<AreaDto> CreateAsync(CreateAreaDto dto);

    Task<AreaDto?> UpdateAsync(int id, UpdateAreaDto dto);

    Task<bool> DeleteAsync(int id);
}

// ==========================================================
// INTERFACE
// ==========================================================
//
// 📋 Think: "Interface = a promise / job description."
//
// It says WHAT a service must be able to do.
// It does NOT do the actual work.
//
// IAreaService says:
// "Any AreaService must be able to:"
//
// GetAllAsync()    → Get all Areas
// GetByIdAsync()   → Get one Area
// CreateAsync()    → Create an Area
// UpdateAsync()    → Update an Area
// DeleteAsync()    → Delete an Area
//
// AreaService says:
// "I promise to do all of these things."
//
// IAreaService = WHAT can be done 📋
// AreaService  = HOW it is done ⚙️
//
// Controller uses IAreaService because it only needs to know
// WHAT it can ask the service to do.
//
// Program.cs connects them:
//
// IAreaService → AreaService
//
// 🔑 Remember:
// Interface = WHAT
// Service    = HOW
//
// ==========================================================

//                 COMPLETE FLOW
//
// 📱 React
//    ↓
// 🚦 Middleware
//    ↓
// 🎯 Controller
//    ↓
// 📦 DTO
//    ↓
// 🛂 Validator
//    ↓
// 📋 Interface
//    ↓
// 🔑 Dependency Injection
//    ↓
// ⚙️ Service
//    ↓
// 🗄️ Database
//
// Then the result comes back:
//
// 🗄️ Database
//    ↓
// ⚙️ Service
//    ↓
// 🎯 Controller
//    ↓
// 📱 React
//
// ==========================================================
//
// 🔑 THE SIMPLE VERSION TO REMEMBER:
//
// DTO        = carries the data 📦
// Validator  = checks the data 🛂
// Controller = directs the request 🎯
// Interface  = says WHAT the service can do 📋
// DI         = gives the Controller the Service 🔑
// Service    = does the actual work ⚙️
// Database   = stores/retrieves data 🗄️
//
// ==========================================================