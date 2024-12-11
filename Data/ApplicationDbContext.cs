using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Models;

using static SocialMediaApp.Models.UserGroup;
using static SocialMediaApp.Models.Follow;

namespace SocialMediaApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set;}
    public DbSet<Post> Posts { get; set;}
    public DbSet<Comment> Comments { get; set;}
    public DbSet<Group> Groups { get; set;}
    public DbSet<Message> Messages { get; set;}
    public DbSet<UserGroup> UserGroups { get; set;}
    public DbSet<Follow> Follows { get; set;}


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        base.OnModelCreating(modelBuilder);

        //relatii many-to-many

        //user - grup
        //cheie primara compusa
        modelBuilder.Entity<UserGroup>().HasKey(a => new {a.Id, a.UserId, a.GroupId});

        //relatii cu modelele User si Group
        modelBuilder.Entity<UserGroup>()
            .HasOne(a => a.User)
            .WithMany(a => a.UserGroup)
            .HasForeignKey(a => a.UserId);
        
        modelBuilder.Entity<UserGroup>()
            .HasOne(a => a.Group)
            .WithMany(a => a.UserGroup)
            .HasForeignKey(a => a.GroupId);

        //user-user
        //cheie primara compusa 
        modelBuilder.Entity<Follow>().HasKey(a => new {a.Id, a.FollowerId, a.FollowedId});

        //relatii cu modelele User si User
        modelBuilder.Entity<Follow>()
            .HasOne(a => a.Follower)
            .WithMany(a => a.Following)
            .HasForeignKey(a => a.FollowerId);
        
        modelBuilder.Entity<Follow>()
            .HasOne(a => a.Followed)
            .WithMany(a => a.Followers)
            .HasForeignKey(a => a.FollowedId);

        // Configurare pentru a evita problemele cu TEXT/BLOB în MySQL
        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(255)").IsRequired();
        });

        modelBuilder.Entity<IdentityUser>(entity =>
        {
            entity.Property(e => e.Id).HasColumnType("varchar(255)").IsRequired();
        });

        modelBuilder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.RoleId).HasColumnType("varchar(255)").IsRequired();
        });

        modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.ProviderKey).HasColumnType("varchar(255)").IsRequired();
        });

        modelBuilder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.LoginProvider).HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.Name).HasColumnType("varchar(255)").IsRequired();
        });
    }
}
