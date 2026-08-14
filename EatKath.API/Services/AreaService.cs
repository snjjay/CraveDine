//Bring me the tools I need to do Area-related work
using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Area; //Data boxes>Database entity
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using EatKath.API.Exceptions;

namespace EatKath.API.Services;

public class AreaService : IAreaService //AreaService is the actual class that performs Area operations.IAreaService implements AreaService
{   
    //What tools does AreaService need to do its job?
    private readonly ApplicationDbContext _context; //I need to talk to the database.
    private readonly IMapper _mapper; //I need to convert between DTOs and database entities
    private readonly IValidator<CreateAreaDto> _createValidator; //I need to check CreateAreaDto
    private readonly IValidator<UpdateAreaDto> _updateValidator; //I need to check UpdateAreaDto

    //Constructor — This is Dependency Injection again.
    public AreaService(
    ApplicationDbContext context,//Database
    IMapper mapper,//Convert between DTO and Entity
    IValidator<CreateAreaDto> createValidator,//Check data
    IValidator<UpdateAreaDto> updateValidator)//Check data
    {
        _context = context;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<AreaDto>> GetAllAsync()
    {
        var areas = await _context.Areas.ToListAsync(); //Give me all Areas from the database

        return _mapper.Map<IEnumerable<AreaDto>>(areas); 
        //Database gives you Area entities
        //But you don't want to send database entities directly to React.
        //So AutoMapper converts: Area Entity>AutoMapper>AreaDto
    }

    public async Task<AreaDto?> GetByIdAsync(int id)
    {
        var area = await _context.Areas.FindAsync(id);

        if (area == null)
        {
            return null;
        }

        return _mapper.Map<AreaDto>(area);
    }




    public async Task<AreaDto> CreateAsync(CreateAreaDto dto) //Create a new Area
    {
        // Validate request
        var validationResult = await _createValidator.ValidateAsync(dto); //Is the data okay

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Check for duplicate area
        var exists = await _context.Areas
            .AnyAsync(a => a.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

        if (exists)
        {
            throw new DuplicateEntityException("Area already exists."); // If not don't continue
        }

        // Create entity
        var area = _mapper.Map<Area>(dto); //Convert DTO → Entity// React sent>CreateAreaDto, But EF Core/database needs:>Area Entity
        //CreateAreaDto>AutoMapper>Area Entity
      
        _context.Areas.Add(area); //Prepare this Area to be inserted into the database

        await _context.SaveChangesAsync(); //Actually save it

        return _mapper.Map<AreaDto>(area); //Convert back to DTO
    }

    public async Task<AreaDto?> UpdateAsync(int id, UpdateAreaDto dto)
    {
        // Validate request
        var validationResult = await _updateValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Find existing area
        var area = await _context.Areas.FindAsync(id);

        if (area == null)
        {
            return null;
        }

        // Check for duplicate name (excluding current record)
        var exists = await _context.Areas.AnyAsync(a =>
            a.Id != id &&
            a.Name.Trim().ToLower() == dto.Name.Trim().ToLower());

        if (exists)
        {
            throw new DuplicateEntityException("Area already exists.");
        }

        // Update entity
        _mapper.Map(dto, area);

        await _context.SaveChangesAsync();

        return _mapper.Map<AreaDto>(area);
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var area = await _context.Areas.FindAsync(id);

        if (area == null)
        {
            return false;
        }

        _context.Areas.Remove(area);

        await _context.SaveChangesAsync();

        return true;
    }


}


// ==========================================================
// AREA SERVICE
// ==========================================================
//
// ⚙️ Think: "The Service does the actual work."
//
// The Controller receives the request and passes the work
// to the Service.
//
// The Service:
// - Gets data from the database
// - Creates data
// - Updates data
// - Deletes data
// - Checks business rules
// - Uses validators
// - Converts DTOs ↔ Database Entities
//
// ----------------------------------------------------------
// REQUEST FLOW
//
// 📱 React
//    ↓
// 📦 DTO
//    "Here is my data"
//    ↓
// 🛂 Validator
//    "Is this data okay?"
//    ↓
// 🎯 Controller
//    "I'll send it to the Service"
//    ↓
// ⚙️ AreaService
//    "I'll do the actual work"
//    ↓
// 🗄️ Database
//    "I'll store/retrieve the data"
//
// ----------------------------------------------------------
// TOOLS USED BY THE SERVICE
//
// 🗄️ _context
//    → Talks to the database
//
// 🔄 _mapper
//    → Converts DTO ↔ Entity
//
// 🛂 _createValidator
//    → Checks CreateAreaDto
//
// 🛂 _updateValidator
//    → Checks UpdateAreaDto
//
// ----------------------------------------------------------
// CREATE FLOW
//
// CreateAreaDto 📦
//       ↓
// Validate 🛂
//       ↓
// Check if Area already exists 🔍
//       ↓
// Convert DTO → Area Entity 🔄
//       ↓
// Save to Database 🗄️
//       ↓
// Convert Area Entity → AreaDto 🔄
//       ↓
// Return to Controller
//
// ----------------------------------------------------------
// UPDATE FLOW
//
// UpdateAreaDto 📦
//       ↓
// Validate 🛂
//       ↓
// Find existing Area 🔍
//       ↓
// Check duplicate name 🔍
//       ↓
// Update Area Entity 🔄
//       ↓
// Save to Database 🗄️
//       ↓
// Convert Entity → AreaDto
//
// ----------------------------------------------------------
// DELETE FLOW
//
// Find Area 🔍
//      ↓
// If not found → return false
//      ↓
// Remove Area 🗑️
//      ↓
// Save to Database 💾
//      ↓
// Return true
//
// ==========================================================
//
// 🔑 REMEMBER:
//
// Controller = receives & directs
// DTO        = carries data
// Validator  = checks data
// Service    = does the actual work
// DbContext  = talks to database
// AutoMapper = converts DTO ↔ Entity
//
// ==========================================================