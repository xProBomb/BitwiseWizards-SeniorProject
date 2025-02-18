using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Data;

public class AuthenticationDbContext : IdentityDbContext
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
        : base(options)
    {
    }
}
