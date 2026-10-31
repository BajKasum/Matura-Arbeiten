using MaturaArbeiten.Models;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<G3SuS> G3SuSListe { get; set; }
    public DbSet<Praesentation> Praesentationen { get; set; }
    public DbSet<Eintragung> Eintragungen { get; set; }
    public DbSet<EmailVersand> EmailVersandListe { get; set; }
    public DbSet<Administrationslogin> Administrationslogins { get; set; }
    public DbSet<Systemeinstellung> Systemeinstellungen { get; set; }
}