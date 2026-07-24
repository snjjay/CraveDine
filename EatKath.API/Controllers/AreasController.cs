using EatKath.API.DTOs.Area;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

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