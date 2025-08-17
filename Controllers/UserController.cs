using AuthApiDemo.Data;
using AuthApiDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add_user")]
        [Authorize]
        public async Task<IActionResult> AddUser([FromBody] User request)
        {
            try
            {
                // Check if user already exists by email
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest("Email already exists");

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Gender = request.Gender,
                    Active = request.Active
                };

                _context.Users.Add(user);
                var result = await _context.SaveChangesAsync();
                
                if (result <= 0)
                    throw new InvalidOperationException("Failed to save user to database. No rows were affected.");

                return Ok(new { user.UserId, user.Email, user.FirstName, user.LastName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        
        [HttpGet("get_users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.UserId, u.Email, u.FirstName, u.LastName, u.Gender, u.Active })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("get_user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new { u.UserId, u.Email, u.FirstName, u.LastName, u.Gender, u.Active })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpDelete("delete_user/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            var result = await _context.SaveChangesAsync();
            
            if (result <= 0)
                throw new InvalidOperationException("Failed to delete user from database. No rows were affected.");

            return Ok(new { message = "User deleted successfully", deletedUserId = id });
        }
    }
}