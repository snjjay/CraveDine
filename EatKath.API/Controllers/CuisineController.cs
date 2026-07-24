using EatKath.API.DTOs.Cuisine;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuisineController : ControllerBase
{
    private readonly ICuisineService _cuisineService;

    public CuisineController(ICuisineService cuisineService)
    {
        _cuisineService = cuisineService;
    }

    // Public - Anyone can view all cuisines
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cuisines = await _cuisineService.GetAllAsync();
        return Ok(cuisines);
    }

    // Public - Anyone can view a single cuisine
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cuisine = await _cuisineService.GetByIdAsync(id);

        if (cuisine == null)
        {
            return NotFound();
        }

        return Ok(cuisine);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCuisineDto dto)
    {
        var createdCuisine = await _cuisineService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdCuisine.Id },
            createdCuisine);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCuisineDto dto)
    {
        var updatedCuisine = await _cuisineService.UpdateAsync(id, dto);

        if (updatedCuisine == null)
        {
            return NotFound();
        }

        return Ok(updatedCuisine);
    }

    // Protected - Admin only
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cuisineService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}