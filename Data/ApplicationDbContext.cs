using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configurare pentru a evita problemele cu TEXT/BLOB în MySQL
        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(255)").IsRequired();
        });

        builder.Entity<IdentityUser>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(255)").IsRequired();
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.RoleId).HasColumnType("varchar(255)").IsRequired();
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.ProviderKey).HasColumnType("varchar(255)").IsRequired();
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.LoginProvider).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.Name).HasColumnType("varchar(255)").IsRequired();
        });
    }
}
