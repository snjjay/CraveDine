using EatKath.API.DTOs.Area;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers;


// ==========================================================
// AreasController
// ==========================================================
//
// 🎯 Think: "Front door of the Areas API."
//
// Controller's job:
// 1. Receive HTTP request
// 2. Check authentication/authorization
// 3. Pass the work to AreaService
// 4. Return HTTP response
//
// Controller does NOT contain the main business/database logic.
// That work belongs in the Service.
//
// REQUEST goes DOWN:
// Client
//    ↓
// Controller
//    ↓
// Service
//    ↓
// Database
//
// ANSWER comes BACK UP:
// Database
//    ↓
// Service
//    ↓
// Controller
//    ↓
// Client
//
// HTTP methods:
// GET    → Read
// POST   → Create
// PUT    → Update
// DELETE → Delete
//
// ==========================================================

[ApiController] //This is an API controller for Areas
[Route("api/[controller]")]  //become /api/Areas
public class AreasController : ControllerBase
{

    //Dependency Injection: The conroller says I need an AreaService(Sancho) ie IAreaService. It doesn't create one.
    //.NET knows what to give it
    //You told .NET in Program.cs:
    //builder.Services.AddScoped<IAreaService, AreaService>(); //When somebody asks for IAreaService, give them an AreaService
    // The controller asks for IAreaService; .NET gives it AreaService; the controller stores it in _areaService.
    
    private readonly IAreaService _areaService; //I have a place called _areaService where I will keep the AreaService that I need.
    public AreasController(IAreaService areaService) //I'm saying Give me the AreaService I need // AddScoped<IAreaService, AreaService>() is telling .NET> I know what to give you.
    {
        _areaService = areaService;
    }

    //

    // Public - Anyone can view all areas
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var areas = await _areaService.GetAllAsync();
        return Ok(areas);
    }

    // Public - Anyone can view a single area
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var area = await _areaService.GetByIdAsync(id);

        if (area == null)
        {
            return NotFound();
        }

        return Ok(area);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAreaDto dto)
    {
        var area = await _areaService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAreaDto dto)
    {
        var area = await _areaService.UpdateAsync(id, dto);

        if (area == null)
        {
            return NotFound();
        }

        return Ok(area);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _areaService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}