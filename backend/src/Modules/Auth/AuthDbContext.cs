using Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth;

public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
}