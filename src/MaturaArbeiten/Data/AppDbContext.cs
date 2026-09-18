using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}