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
		private readonly SignInManager<IdentityUser> _signInManager;

		public UserController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] LoginDto request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
			}

			if (request == null)
			{
				return BadRequest(new { message = "Request body is required" });
			}

			var email = request.Email?.Trim();
			var password = request.Password;

			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest(new { message = "Email is required" });
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				return BadRequest(new { message = "Password is required" });
			}

			try
			{
				var user = await _userManager.FindByEmailAsync(email);

				if (user == null)
				{
					return Unauthorized(new { message = "Invalid email or password" });
				}

				if (await _userManager.IsLockedOutAsync(user))
				{
					return Unauthorized(new { message = "Account is locked. Please try again later" });
				}

				var passwordValid = await _userManager.CheckPasswordAsync(user, password);

				if (!passwordValid)
				{
					await _userManager.AccessFailedAsync(user);
					var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);

					if (failedAttempts >= 5)
					{
						await _userManager.SetLockoutEnabledAsync(user, true);
						await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
					}

					return Unauthorized(new { message = "Invalid email or password" });
				}

				await _userManager.ResetAccessFailedCountAsync(user);

				var roles = await _userManager.GetRolesAsync(user);

				return Ok(new
				{
					message = "Login successful",
					userId = user.Id,
					email = user.Email,
					username = user.UserName,
					roles = roles
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred during login" });
			}
		}

		[HttpPost("add")]
		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
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
		[Authorize]
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

		[HttpPut("update-role/{userId}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDto request)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest(new { message = "User ID is required" });
			}

			if (string.IsNullOrWhiteSpace(request.NewRole))
			{
				return BadRequest(new { message = "New role is required" });
			}

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new { message = "User not found" });
			}

			// Get current roles
			var currentRoles = await _userManager.GetRolesAsync(user);

			// Remove all current roles
			if (currentRoles.Count > 0)
			{
				var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
				if (!removeResult.Succeeded)
				{
					return BadRequest(new { errors = removeResult.Errors.Select(e => e.Description) });
				}
			}

			// Add new role
			var addResult = await _userManager.AddToRoleAsync(user, request.NewRole.Trim());
			if (!addResult.Succeeded)
			{
				return BadRequest(new { errors = addResult.Errors.Select(e => e.Description) });
			}

			return Ok(new { message = "User role updated successfully", userId = user.Id, newRole = request.NewRole });
		}
	}
}
