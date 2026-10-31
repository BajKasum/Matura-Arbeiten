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

        modelBuilder.Entity<Praesentation>(entity =>
        {
            entity.ToTable("praesentation");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Thema).HasColumnName("thema").HasMaxLength(200);
            entity.Property(x => x.Datum).HasColumnName("datum");
            entity.Property(x => x.Uhrzeit).HasColumnName("uhrzeit").HasColumnType("time(0)");
            entity.Property(x => x.Raum).HasColumnName("raum").HasMaxLength(20);
        });

        modelBuilder.Entity<Eintragung>(entity =>
        {
            entity.ToTable("eintragung");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.G3susId).HasColumnName("g3sus_id");
            entity.Property(x => x.PraesentationId).HasColumnName("praesentation_id");
            entity.Property(x => x.ErstelltAm).HasColumnName("erstellt_am");

            entity.HasOne<G3SuS>()
                .WithMany()
                .HasForeignKey(x => x.G3susId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Praesentation>()
                .WithMany(p => p.Eintragungen)
                .HasForeignKey(x => x.PraesentationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.G3susId, x.PraesentationId }).IsUnique();
        });

        modelBuilder.Entity<EmailVersand>(entity =>
        {
            entity.ToTable("email_versand");

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.G3susId).HasColumnName("g3sus_id");
            entity.Property(x => x.Typ).HasColumnName("typ").HasMaxLength(20);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(x => x.GesendetAm).HasColumnName("gesendet_am");

            entity.HasOne<G3SuS>()
                .WithMany()
                .HasForeignKey(x => x.G3susId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}