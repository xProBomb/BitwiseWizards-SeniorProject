using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

public partial class TrustTradeDbContext : DbContext
{
    public TrustTradeDbContext()
    {
    }

    public TrustTradeDbContext(DbContextOptions<TrustTradeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Trade> Trades { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer("Name=TrustTradeConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC2783103CE7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments).HasConstraintName("FK_Comments_Post");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Follower__3214EC27F5781985");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FollowerUser).WithMany(p => p.FollowerFollowerUsers).HasConstraintName("FK_Followers_Follower");

            entity.HasOne(d => d.FollowingUser).WithMany(p => p.FollowerFollowingUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Followers_Following");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Likes__3214EC27C0B9E6BA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Likes_Post");

            entity.HasOne(d => d.User).WithMany(p => p.Likes).HasConstraintName("FK_Likes_User");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Posts__3214EC2756A2A74A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PrivacySetting).HasDefaultValue("Public");

            entity.HasOne(d => d.User).WithMany(p => p.Posts).HasConstraintName("FK_Posts_User");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stock__3214EC2757CBA2DB");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trade__3214EC273E6E3285");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.TickerSymbolNavigation).WithMany(p => p.Trades)
                .HasPrincipalKey(p => p.TickerSymbol)
                .HasForeignKey(d => d.TickerSymbol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trade_Stock");

            entity.HasOne(d => d.User).WithMany(p => p.Trades).HasConstraintName("FK_Trade_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC2767B019D4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
