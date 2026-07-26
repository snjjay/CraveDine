using EatKath.API.DTOs.UserFavorite;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatKath.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserFavoriteController : ControllerBase
    {
        private readonly IUserFavoriteService _service;

        public UserFavoriteController(IUserFavoriteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            return Ok(await _service.GetMyFavoritesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateUserFavoriteDto dto)
        {
            await _service.AddAsync(dto);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Remove([FromBody] RemoveUserFavoriteDto dto)
        {
            await _service.RemoveAsync(dto);

            return NoContent();
        }
    }
}