using Microsoft.AspNetCore.Mvc;
using MindLog.Api.Core.Application.Services.DTOs;
using MindLog.Api.Core.Domain.Interfaces;

namespace MindLog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileDto request)
        {
            try
            {
                var success = await _userService.UpdateProfileAsync(id, request);
                
                if (!success)
                    return NotFound(new { message = "Usuario no encontrado." });

                return Ok(new { message = "Perfil actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor.", details = ex.Message });
            }
        }
    }
}