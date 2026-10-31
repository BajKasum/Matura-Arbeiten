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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<G3SuS>(entity =>
        {
            entity.ToTable("g3sus");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Vorname).HasColumnName("vorname").HasMaxLength(50);
            entity.Property(x => x.Nachname).HasColumnName("nachname").HasMaxLength(50);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(x => x.PasswortHash).HasColumnName("passwort_hash").HasMaxLength(255);
            entity.Property(x => x.PasswortGeaendert).HasColumnName("passwort_geaendert");
            entity.Property(x => x.Fehlversuche).HasColumnName("fehlversuche");
            entity.Property(x => x.GesperrtBis).HasColumnName("gesperrt_bis");

            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}