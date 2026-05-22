using Microsoft.AspNetCore.Identity;

namespace backend.Data;

public class ApplicationUser : IdentityUser
{
    public string permissions { get; set; } = string.Empty;
}