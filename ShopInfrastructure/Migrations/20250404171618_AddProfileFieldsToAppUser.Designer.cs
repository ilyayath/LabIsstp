﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ShopMVC.ShopInfrastructure;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    [DbContext(typeof(MerchShopeContext))]
    [Migration("20250404171618_AddProfileFieldsToAppUser")]
    partial class AddProfileFieldsToAppUser
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("ShopDomain.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<string>("ShippingAddress")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("ShopDomain.Models.Brand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BrandName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("brands_pkey");

                    b.ToTable("Brands");
                });

            modelBuilder.Entity("ShopDomain.Models.Buyer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("character varying");

                    b.Property<int?>("CityId")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("buyers_pkey");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.HasIndex(new[] { "CityId" }, "IX_Buyers_CityId");

                    b.HasIndex(new[] { "Username" }, "buyers_username_key")
                        .IsUnique();

                    b.ToTable("Buyers");
                });

            modelBuilder.Entity("ShopDomain.Models.CartItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MerchandiseId")
                        .HasColumnType("integer");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CartItems");
                });

            modelBuilder.Entity("ShopDomain.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("categories_pkey");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ShopDomain.Models.City", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<string>("CityName")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("Cities_pkey");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("ShopDomain.Models.MerchOrder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("BuyerId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("OrderDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int?>("PaymentId")
                        .HasColumnType("integer");

                    b.Property<int?>("ShipmentId")
                        .HasColumnType("integer");

                    b.Property<int?>("StatusId")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("merchorders_pkey");

                    b.HasIndex("BuyerId");

                    b.HasIndex(new[] { "PaymentId" }, "IX_MerchOrders_PaymentId");

                    b.HasIndex(new[] { "ShipmentId" }, "IX_MerchOrders_ShipmentId");

                    b.HasIndex(new[] { "StatusId" }, "IX_MerchOrders_StatusId");

                    b.HasIndex(new[] { "UserId" }, "IX_MerchOrders_UserId");

                    b.ToTable("MerchOrders");
                });

            modelBuilder.Entity("ShopDomain.Models.Merchandise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("BrandId")
                        .HasColumnType("integer");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValueSql("''::text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<decimal>("Price")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<int?>("SizeId")
                        .HasColumnType("integer");

                    b.Property<int?>("TeamId")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("merchandises_pkey");

                    b.HasIndex(new[] { "BrandId" }, "IX_Merchandises_BrandId");

                    b.HasIndex(new[] { "CategoryId" }, "IX_Merchandises_CategoryId");

                    b.HasIndex(new[] { "SizeId" }, "IX_Merchandises_SizeId");

                    b.HasIndex(new[] { "TeamId" }, "IX_Merchandises_TeamId");

                    b.ToTable("Merchandises");
                });

            modelBuilder.Entity("ShopDomain.Models.OrderItem", b =>
                {
                    b.Property<int>("OrderId")
                        .HasColumnType("integer");

                    b.Property<int>("MerchId")
                        .HasColumnType("integer");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.HasKey("OrderId", "MerchId")
                        .HasName("orderitems_pkey");

                    b.HasIndex(new[] { "MerchId" }, "IX_OrderItems_MerchId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("ShopDomain.Models.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("StatusName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("orderstatus_pkey");

                    b.ToTable("OrderStatuses");
                });

            modelBuilder.Entity("ShopDomain.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("TypePayment")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("payments_pkey");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("ShopDomain.Models.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BuyerId")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .HasColumnType("character varying");

                    b.Property<int>("MerchandiseId")
                        .HasColumnType("integer");

                    b.Property<int?>("Rating")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReviewDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Id")
                        .HasName("Reviews_pkey");

                    b.HasIndex(new[] { "BuyerId" }, "IX_Reviews_BuyerId");

                    b.HasIndex(new[] { "MerchandiseId" }, "IX_Reviews_MerchandiseId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("ShopDomain.Models.Shipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("TypeShipment")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("shipments_pkey");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("ShopDomain.Models.Size", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("SizeName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id")
                        .HasName("sizes_pkey");

                    b.ToTable("Sizes");
                });

            modelBuilder.Entity("ShopDomain.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id")
                        .HasName("teams_pkey");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("ShopDomain.Models.UserCart", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MerchandiseId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("usercarts_pkey");

                    b.HasIndex(new[] { "MerchandiseId" }, "IX_UserCarts_MerchandiseId");

                    b.HasIndex(new[] { "UserId" }, "IX_UserCarts_UserId");

                    b.ToTable("UserCarts");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ShopDomain.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ShopDomain.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShopDomain.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("ShopDomain.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShopDomain.Models.Buyer", b =>
                {
                    b.HasOne("ShopDomain.Models.City", "City")
                        .WithMany("Buyers")
                        .HasForeignKey("CityId")
                        .HasConstraintName("fr_city");

                    b.HasOne("ShopDomain.Models.AppUser", "User")
                        .WithOne()
                        .HasForeignKey("ShopDomain.Models.Buyer", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("City");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShopDomain.Models.MerchOrder", b =>
                {
                    b.HasOne("ShopDomain.Models.Buyer", null)
                        .WithMany("MerchOrders")
                        .HasForeignKey("BuyerId");

                    b.HasOne("ShopDomain.Models.Payment", "Payment")
                        .WithMany("MerchOrders")
                        .HasForeignKey("PaymentId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchorders_paymentid_fkey");

                    b.HasOne("ShopDomain.Models.Shipment", "Shipment")
                        .WithMany("MerchOrders")
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchorders_shipmentid_fkey");

                    b.HasOne("ShopDomain.Models.OrderStatus", "Status")
                        .WithMany("MerchOrders")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchorders_statusid_fkey");

                    b.HasOne("ShopDomain.Models.AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("merchorders_userid_fkey");

                    b.Navigation("Payment");

                    b.Navigation("Shipment");

                    b.Navigation("Status");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShopDomain.Models.Merchandise", b =>
                {
                    b.HasOne("ShopDomain.Models.Brand", "Brand")
                        .WithMany("Merchandises")
                        .HasForeignKey("BrandId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchandises_brandid_fkey");

                    b.HasOne("ShopDomain.Models.Category", "Category")
                        .WithMany("Merchandises")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchandises_categoryid_fkey");

                    b.HasOne("ShopDomain.Models.Size", "Size")
                        .WithMany("Merchandises")
                        .HasForeignKey("SizeId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchandises_sizeid_fkey");

                    b.HasOne("ShopDomain.Models.Team", "Team")
                        .WithMany("Merchandises")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("merchandises_teamid_fkey");

                    b.Navigation("Brand");

                    b.Navigation("Category");

                    b.Navigation("Size");

                    b.Navigation("Team");
                });

            modelBuilder.Entity("ShopDomain.Models.OrderItem", b =>
                {
                    b.HasOne("ShopDomain.Models.Merchandise", "Merch")
                        .WithMany("OrderItems")
                        .HasForeignKey("MerchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("orderitems_merchid_fkey");

                    b.HasOne("ShopDomain.Models.MerchOrder", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("orderitems_orderid_fkey");

                    b.Navigation("Merch");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("ShopDomain.Models.Review", b =>
                {
                    b.HasOne("ShopDomain.Models.Buyer", "Buyer")
                        .WithMany("Reviews")
                        .HasForeignKey("BuyerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("reviews_buyerid_fkey");

                    b.HasOne("ShopDomain.Models.Merchandise", "Merchandise")
                        .WithMany("Reviews")
                        .HasForeignKey("MerchandiseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("reviews_merchandiseid_fkey");

                    b.Navigation("Buyer");

                    b.Navigation("Merchandise");
                });

            modelBuilder.Entity("ShopDomain.Models.UserCart", b =>
                {
                    b.HasOne("ShopDomain.Models.Merchandise", "Merchandise")
                        .WithMany("UserCarts")
                        .HasForeignKey("MerchandiseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("usercarts_merchandiseid_fkey");

                    b.Navigation("Merchandise");
                });

            modelBuilder.Entity("ShopDomain.Models.Brand", b =>
                {
                    b.Navigation("Merchandises");
                });

            modelBuilder.Entity("ShopDomain.Models.Buyer", b =>
                {
                    b.Navigation("MerchOrders");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("ShopDomain.Models.Category", b =>
                {
                    b.Navigation("Merchandises");
                });

            modelBuilder.Entity("ShopDomain.Models.City", b =>
                {
                    b.Navigation("Buyers");
                });

            modelBuilder.Entity("ShopDomain.Models.MerchOrder", b =>
                {
                    b.Navigation("OrderItems");
                });

            modelBuilder.Entity("ShopDomain.Models.Merchandise", b =>
                {
                    b.Navigation("OrderItems");

                    b.Navigation("Reviews");

                    b.Navigation("UserCarts");
                });

            modelBuilder.Entity("ShopDomain.Models.OrderStatus", b =>
                {
                    b.Navigation("MerchOrders");
                });

            modelBuilder.Entity("ShopDomain.Models.Payment", b =>
                {
                    b.Navigation("MerchOrders");
                });

            modelBuilder.Entity("ShopDomain.Models.Shipment", b =>
                {
                    b.Navigation("MerchOrders");
                });

            modelBuilder.Entity("ShopDomain.Models.Size", b =>
                {
                    b.Navigation("Merchandises");
                });

            modelBuilder.Entity("ShopDomain.Models.Team", b =>
                {
                    b.Navigation("Merchandises");
                });
#pragma warning restore 612, 618
        }
    }
}
