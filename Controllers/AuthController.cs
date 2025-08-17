using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthApiDemo.Domain.Interfaces;
using AuthApiDemo.Application.DTOs;
using AuthApiDemo.Models;

namespace AuthApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.RegisterAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    request.Gender);

                if (!result.Success)
                {
                    return BadRequest(new RegisterResultDto
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                var userDto = new UserDto
                {
                    UserId = result.User!.UserId,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    Email = result.User.Email,
                    Gender = result.User.Gender,
                    Active = result.User.Active,
                    CreatedAt = result.User.CreatedAt,
                    UpdatedAt = result.User.UpdatedAt,
                    FullName = result.User.GetFullName()
                };

                return Ok(new RegisterResultDto
                {
                    Success = true,
                    Message = result.Message,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RegisterResultDto
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _authService.LoginAsync(request.Email, request.Password);

                if (!result.Success)
                {
                    return Unauthorized(new LoginResponse
                    {
                        IsAuthenticated = false,
                        Message = result.Message
                    });
                }

                return Ok(new LoginResponse
                {
                    Token = result.Token!,
                    RefreshToken = result.RefreshToken!,
                    Expiration = result.Expiration!.Value,
                    Email = result.User!.Email,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    IsAuthenticated = true,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    IsAuthenticated = false,
                    Message = "Internal server error"
                });
            }
        }

        [Authorize]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _authService.RefreshTokenAsync(userId, refreshToken);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(new AuthResponse
                {
                    Token = result.Token!,
                    RefreshToken = result.RefreshToken!,
                    Expiration = result.Expiration!.Value
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to change password. Please verify your current password." });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    // Additional request model for change password
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
