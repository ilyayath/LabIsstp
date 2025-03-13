using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Models;

namespace ShopMVC.ShopInfrastructure
{
    public partial class MerchShopeContext : IdentityDbContext<AppUser>
    {
        public MerchShopeContext()
        {
        }

        public MerchShopeContext(DbContextOptions<MerchShopeContext> options)
            : base(options)
        {
        }

        public DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Buyer> Buyers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<MerchOrder> MerchOrders { get; set; }
        public virtual DbSet<Merchandise> Merchandises { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Shipment> Shipments { get; set; }
        public virtual DbSet<Size> Sizes { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<UserCart> UserCarts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=merch_shope;Username=postgres;Password=googlemaybeop314");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Обов’язковий виклик базового методу для Identity
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Buyer>()
                .HasOne(b => b.User)
                .WithOne()
                .HasForeignKey<Buyer>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Buyer>()
                .HasMany(b => b.Reviews)
                .WithOne(r => r.Buyer)
                .HasForeignKey(r => r.BuyerId);

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("brands_pkey");
                entity.Property(e => e.BrandName).HasMaxLength(255);
            });

            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("buyers_pkey");
                entity.HasIndex(e => e.CityId, "IX_Buyers_CityId");
                entity.HasIndex(e => e.Username, "buyers_username_key").IsUnique();
                entity.Property(e => e.Address).HasColumnType("character varying");
                entity.Property(e => e.Username).HasMaxLength(255);
                entity.HasOne(d => d.City).WithMany(p => p.Buyers)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("fr_city");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("categories_pkey");
                entity.Property(e => e.CategoryName).HasMaxLength(255);
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("Cities_pkey");
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.CityName).HasMaxLength(255);
            });

            modelBuilder.Entity<MerchOrder>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("merchorders_pkey");
                entity.HasIndex(e => e.UserId, "IX_MerchOrders_UserId"); // Змінено з BuyerId
                entity.HasIndex(e => e.PaymentId, "IX_MerchOrders_PaymentId");
                entity.HasIndex(e => e.ShipmentId, "IX_MerchOrders_ShipmentId");
                entity.HasIndex(e => e.StatusId, "IX_MerchOrders_StatusId");
                entity.Property(e => e.OrderDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone");
                entity.HasOne(d => d.User) // Змінено з Buyer на User
                    .WithMany()
                    .HasForeignKey(d => d.UserId) // Змінено з BuyerId на UserId
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("merchorders_userid_fkey");
                entity.HasOne(d => d.Payment).WithMany(p => p.MerchOrders)
                    .HasForeignKey(d => d.PaymentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchorders_paymentid_fkey");
                entity.HasOne(d => d.Shipment).WithMany(p => p.MerchOrders)
                    .HasForeignKey(d => d.ShipmentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchorders_shipmentid_fkey");
                entity.HasOne(d => d.Status).WithMany(p => p.MerchOrders)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchorders_statusid_fkey");
            });

            modelBuilder.Entity<Merchandise>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("merchandises_pkey");
                entity.HasIndex(e => e.BrandId, "IX_Merchandises_BrandId");
                entity.HasIndex(e => e.CategoryId, "IX_Merchandises_CategoryId");
                entity.HasIndex(e => e.SizeId, "IX_Merchandises_SizeId");
                entity.HasIndex(e => e.TeamId, "IX_Merchandises_TeamId");
                entity.Property(e => e.ImageUrl).HasDefaultValueSql("''::text");
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.HasOne(d => d.Brand).WithMany(p => p.Merchandises)
                    .HasForeignKey(d => d.BrandId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchandises_brandid_fkey");
                entity.HasOne(d => d.Category).WithMany(p => p.Merchandises)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchandises_categoryid_fkey");
                entity.HasOne(d => d.Size).WithMany(p => p.Merchandises)
                    .HasForeignKey(d => d.SizeId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchandises_sizeid_fkey");
                entity.HasOne(d => d.Team).WithMany(p => p.Merchandises)
                    .HasForeignKey(d => d.TeamId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("merchandises_teamid_fkey");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.MerchId }).HasName("orderitems_pkey");
                entity.HasIndex(e => e.MerchId, "IX_OrderItems_MerchId");
                entity.HasOne(d => d.Merch).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.MerchId)
                    .HasConstraintName("orderitems_merchid_fkey");
                entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("orderitems_orderid_fkey");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("orderstatus_pkey");
                entity.Property(e => e.StatusName).HasMaxLength(255);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("payments_pkey");
                entity.Property(e => e.TypePayment).HasMaxLength(255);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("Reviews_pkey");
                entity.HasIndex(e => e.BuyerId, "IX_Reviews_BuyerId");
                entity.HasIndex(e => e.MerchandiseId, "IX_Reviews_MerchandiseId");
                entity.Property(e => e.Comment).HasColumnType("character varying");
                entity.Property(e => e.ReviewDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone");
                entity.HasOne(d => d.Buyer).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.BuyerId)
                    .HasConstraintName("reviews_buyerid_fkey");
                entity.HasOne(d => d.Merchandise).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.MerchandiseId)
                    .HasConstraintName("reviews_merchandiseid_fkey");
            });

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("shipments_pkey");
                entity.Property(e => e.TypeShipment).HasMaxLength(255);
            });

            modelBuilder.Entity<Size>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("sizes_pkey");
                entity.Property(e => e.SizeName).HasMaxLength(50);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("teams_pkey");
                entity.Property(e => e.TeamName).HasMaxLength(255);
            });

            modelBuilder.Entity<UserCart>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("usercarts_pkey");
                entity.HasIndex(e => e.MerchandiseId, "IX_UserCarts_MerchandiseId");
                entity.HasIndex(e => e.UserId, "IX_UserCarts_UserId");
                entity.HasOne(d => d.Merchandise).WithMany(p => p.UserCarts)
                    .HasForeignKey(d => d.MerchandiseId)
                    .HasConstraintName("usercarts_merchandiseid_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}