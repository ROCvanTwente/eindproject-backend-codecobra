using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserManager<IdentityUser> _userManager;

		public UserController(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpPost("add")]
		public async Task<IActionResult> AddUser([FromBody] CreateUserDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = new IdentityUser
			{
				UserName = request.Username,
				Email = request.Email
			};

			var result = await _userManager.CreateAsync(user, request.Password);

			if (!result.Succeeded)
			{
				return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
			}

			// Add role to user if needed (requires RoleManager setup)
			if (!string.IsNullOrEmpty(request.Role))
			{
				var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
				if (!roleResult.Succeeded)
				{
					// User created but role assignment failed
					return BadRequest(new { message = "User created but role assignment failed", errors = roleResult.Errors.Select(e => e.Description) });
				}
			}

			return Ok(new { message = "User created successfully", userId = user.Id });
		}

		[HttpDelete("delete/{userId}")]
		public async Task<IActionResult> DeleteUser(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest(new { message = "User ID is required" });
			}

			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				return NotFound(new { message = "User not found" });
			}

			var result = await _userManager.DeleteAsync(user);

			if (!result.Succeeded)
			{
				return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
			}

			return Ok(new { message = "User deleted successfully" });
		}

		[HttpGet("all")]
		public async Task<IActionResult> GetAllUsers()
		{
			var userList = new List<UserResponseDto>();

			foreach (var user in _userManager.Users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				userList.Add(new UserResponseDto
				{
					Id = user.Id,
					Username = user.UserName,
					Email = user.Email,
					Role = roles.FirstOrDefault() ?? "Editor"
				});
			}

			return Ok(userList);
		}
	}
}
