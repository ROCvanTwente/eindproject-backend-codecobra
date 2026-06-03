using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

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

			// Assign role (if provided). If role assignment fails, roll back user creation.
			if (!string.IsNullOrWhiteSpace(request.Role))
			{
				var role = request.Role.Trim();
				var roleResult = await _userManager.AddToRoleAsync(user, role);
				if (!roleResult.Succeeded)
				{
					await _userManager.DeleteAsync(user);
					return BadRequest(new
					{
						message = "User created but role assignment failed (user creation rolled back)",
						errors = roleResult.Errors.Select(e => e.Description)
					});
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
			var users = await _userManager.Users.ToListAsync();

			var userList = new List<UserResponseDto>(users.Count);
			foreach (var user in users)
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

		[HttpGet("me")]
		[AllowAnonymous]
		public async Task<IActionResult> GetCurrentUserInfo()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Unauthorized(new { message = "Not authenticated" });
			}

			var roles = await _userManager.GetRolesAsync(user);
			return Ok(new
			{
				userId = user.Id,
				username = user.UserName,
				email = user.Email,
				roles = roles,
				isAdmin = roles.Contains("Admin")
			});
		}
	}
}
