using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;

namespace TrustTrade.Context;

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

    public virtual DbSet<InvestmentPosition> InvestmentPositions { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<PlaidConnection> PlaidConnections { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Trade> Trades { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:trusttrade.database.windows.net,1433;Initial Catalog=TrustTrade-DB;Persist Security Info=False;User ID=resourceadmin;Password=WouWolves99!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC27F40245E7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Follower__3214EC27F91512A2");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<InvestmentPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Investme__3214EC27E2A465AA");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Likes__3214EC2754DB71E6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<PlaidConnection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlaidCon__3214EC270FDE352D");

            entity.Property(e => e.LastSyncTimestamp).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Posts__3214EC27B8094D9F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PrivacySetting).HasDefaultValue("Public");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stock__3214EC275CD17266");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trade__3214EC27C999C95A");

            entity.Property(e => e.LastUpdated).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC276FA1AE70");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.PlaidEnabled).HasDefaultValue(false);
            entity.Property(e => e.PlaidStatus).HasDefaultValue("Not Connected");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
