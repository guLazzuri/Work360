using Microsoft.AspNetCore.Mvc;
using Work360.Domain.DTO;
using Work360.Domain.Services;

namespace Work360.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var response = await _authService.Authenticate(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            return Ok(response);
        }
    }
}
