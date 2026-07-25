using EatKath.API.DTOs.MenuItem;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;

        public MenuItemController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        // GET: api/MenuItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll()
        {
            var items = await _menuItemService.GetAllAsync();
            return Ok(items);
        }

        // GET: api/MenuItem/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetById(int id)
        {
            var item = await _menuItemService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // GET: api/MenuItem/restaurant/12
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetByRestaurant(int restaurantId)
        {
            var items = await _menuItemService.GetByRestaurantAsync(restaurantId);
            return Ok(items);
        }

        // GET: api/MenuItem/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetByCategory(int categoryId)
        {
            var items = await _menuItemService.GetByCategoryAsync(categoryId);
            return Ok(items);
        }

        // POST: api/MenuItem
        [HttpPost]
        public async Task<ActionResult<MenuItemDto>> Create(CreateMenuItemDto dto)
        {
            var item = await _menuItemService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        // PUT: api/MenuItem/5
        [HttpPut("{id}")]
        public async Task<ActionResult<MenuItemDto>> Update(int id, UpdateMenuItemDto dto)
        {
            var item = await _menuItemService.UpdateAsync(id, dto);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // DELETE: api/MenuItem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _menuItemService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}