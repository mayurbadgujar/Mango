using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService)
        {
            _authService = authService;
            _response = new();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            var errorMsg = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                _response.IsSuccess = false;
                _response.Message = errorMsg;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "User or Password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> Login([FromBody] RegistrationRequestDto model)
        {
            var roleResponse = await _authService.AssignRole(model.Email,model.RoleName);
            if (!roleResponse)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encounter";
                return BadRequest(_response);
            }
            _response.Result = roleResponse;
            return Ok(_response);
        }
    }
}
