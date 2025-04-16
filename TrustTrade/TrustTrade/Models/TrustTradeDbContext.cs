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

    public virtual DbSet<InvestmentPosition> InvestmentPositions { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<PlaidConnection> PlaidConnections { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Trade> Trades { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<PortfolioVisibilitySettings> PortfolioVisibilitySettings { get; set; }
    
    public virtual DbSet<VerificationHistory> VerificationHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC27F40245E7");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_Comments_Post");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_User");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Follower__3214EC27F91512A2");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FollowerUserId).HasColumnName("FollowerUserID");
            entity.Property(e => e.FollowingUserId).HasColumnName("FollowingUserID");

            entity.HasOne(d => d.FollowerUser).WithMany(p => p.FollowerFollowerUsers)
                .HasForeignKey(d => d.FollowerUserId)
                .HasConstraintName("FK_Followers_Follower");

            entity.HasOne(d => d.FollowingUser).WithMany(p => p.FollowerFollowingUsers)
                .HasForeignKey(d => d.FollowingUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Followers_Following");
        });

        modelBuilder.Entity<InvestmentPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Investme__3214EC27E2A465AA");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CostBasis).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PlaidConnectionId).HasColumnName("PlaidConnectionID");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 8)");
            entity.Property(e => e.SecurityId)
                .HasMaxLength(100)
                .HasColumnName("SecurityID");
            entity.Property(e => e.Symbol).HasMaxLength(10);
            entity.Property(e => e.IsHidden)
                .HasDefaultValue(false);

            entity.HasOne(d => d.PlaidConnection).WithMany(p => p.InvestmentPositions)
                .HasForeignKey(d => d.PlaidConnectionId)
                .HasConstraintName("FK_InvestmentPositions_PlaidConnection");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Likes__3214EC2754DB71E6");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Likes_Post");

            entity.HasOne(d => d.User).WithMany(p => p.Likes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Likes_User");
        });

        modelBuilder.Entity<PlaidConnection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlaidCon__3214EC270FDE352D");

            entity.HasIndex(e => e.ItemId, "UQ__PlaidCon__727E83EAA3D35DB3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.InstitutionId)
                .HasMaxLength(50)
                .HasColumnName("InstitutionID");
            entity.Property(e => e.InstitutionName).HasMaxLength(100);
            entity.Property(e => e.ItemId)
                .HasMaxLength(100)
                .HasColumnName("ItemID");
            entity.Property(e => e.LastSyncTimestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.PlaidConnections)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PlaidConnections_User");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Posts__3214EC27B8094D9F");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(128);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.PortfolioValueAtPosting)
                .HasColumnType("decimal(18, 2)")
                .IsRequired(false);

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Posts_User");

            entity.HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "PostTags",
                j => j.HasOne<Tag>().WithMany()
                    .HasForeignKey("TagID")
                    .HasConstraintName("FK_PostTags_Tag")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Post>().WithMany()
                    .HasForeignKey("PostID")
                    .HasConstraintName("FK_PostTags_Post")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("PostID", "TagID").HasName("PK__PostTags__7C45AF9C52906CEA");
                    j.ToTable("PostTags");
                    j.Property<int>("PostID").HasColumnName("PostID");
                    j.Property<int>("TagID").HasColumnName("TagID");
                });
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stock__3214EC275CD17266");

            entity.ToTable("Stock");

            entity.HasIndex(e => e.TickerSymbol, "UQ__Stock__F144591B01402E14").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DailyChange).HasColumnType("decimal(13, 2)");
            entity.Property(e => e.StockPrice).HasColumnType("decimal(13, 2)");
            entity.Property(e => e.TickerSymbol)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC2786A0DFD7");

            entity.HasIndex(e => e.TagName, "UQ__Tags__A2F1B0E1A3A3D3A4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TagName)
                .HasColumnName("TagName")
                .HasMaxLength(100)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Trade__3214EC27C999C95A");

            entity.ToTable("Trade");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(13, 2)");
            entity.Property(e => e.EntryPrice).HasColumnType("decimal(13, 2)");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TickerSymbol)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TradeType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.TickerSymbolNavigation).WithMany(p => p.Trades)
                .HasPrincipalKey(p => p.TickerSymbol)
                .HasForeignKey(d => d.TickerSymbol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trade_Stock");

            entity.HasOne(d => d.User).WithMany(p => p.Trades)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Trade_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC276FA1AE70");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E43599A86E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534AB940664").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdentityId).HasColumnName("IdentityId");
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EncryptedApikey).HasColumnName("EncryptedAPIKey");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("Is_Admin");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("Is_Verified");
            entity.Property(e => e.LastPlaidSync).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PlaidEnabled).HasDefaultValue(false);
            entity.Property(e => e.PlaidStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Not Connected");
            entity.Property(e => e.ProfileName)
                .HasMaxLength(50)
                .HasColumnName("Profile_Name");
            entity.Property(e => e.ProfilePicture).HasColumnType("varbinary(max)");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<PortfolioVisibilitySettings>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User)
                .WithOne()
                .HasForeignKey<PortfolioVisibilitySettings>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<VerificationHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VerificationHistory__3214EC27");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason)
                .HasMaxLength(255);
            entity.Property(e => e.Source)
                .HasMaxLength(50);

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_VerificationHistory_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
