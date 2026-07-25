using EatKath.API.DTOs.MenuCategory;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuCategoryController : ControllerBase
    {
        private readonly IMenuCategoryService _menuCategoryService;

        public MenuCategoryController(IMenuCategoryService menuCategoryService)
        {
            _menuCategoryService = menuCategoryService;
        }

        // GET: api/MenuCategory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuCategoryDto>>> GetAll()
        {
            var categories = await _menuCategoryService.GetAllAsync();
            return Ok(categories);
        }

        // GET: api/MenuCategory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuCategoryDto>> GetById(int id)
        {
            var category = await _menuCategoryService.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // GET: api/MenuCategory/restaurant/12
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<ActionResult<IEnumerable<MenuCategoryDto>>> GetByRestaurant(int restaurantId)
        {
            var categories = await _menuCategoryService.GetByRestaurantAsync(restaurantId);
            return Ok(categories);
        }

        // POST: api/MenuCategory
        [HttpPost]
        public async Task<ActionResult<MenuCategoryDto>> Create(CreateMenuCategoryDto dto)
        {
            var category = await _menuCategoryService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                category);
        }

        // PUT: api/MenuCategory/5
        [HttpPut("{id}")]
        public async Task<ActionResult<MenuCategoryDto>> Update(int id, UpdateMenuCategoryDto dto)
        {
            var category = await _menuCategoryService.UpdateAsync(id, dto);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // DELETE: api/MenuCategory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _menuCategoryService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}