﻿// <auto-generated />
using System;
using Identity.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Identity.API.Migrations
{
    [DbContext(typeof(IdentityDBContext))]
    [Migration("20200917051651_UserPermissionAndGroup_TablesAdded")]
    partial class UserPermissionAndGroup_TablesAdded
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Identity.API.Entities.AppGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("AppGroup");
                });

            modelBuilder.Entity("Identity.API.Entities.AppGroupPermission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<int>("AppGroupId")
                        .HasColumnType("int");

                    b.Property<int>("PermissionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppGroupId");

                    b.HasIndex("PermissionId");

                    b.ToTable("AppGroupPermission");
                });

            modelBuilder.Entity("Identity.API.Entities.AppPermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("AppPermission");
                });

            modelBuilder.Entity("Identity.API.Entities.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<DateTime?>("CreatedDateTimeUtc")
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.Property<bool>("EmailConfirmed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100)
                        .IsUnicode(false);

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100)
                        .IsUnicode(false);

                    b.Property<bool>("LockoutEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LockoutEndDateTimeUtc")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("ModifiedDateTimeUtc")
                        .HasColumnType("datetime");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.ToTable("AppUser");
                });

            modelBuilder.Entity("Identity.API.Entities.AppUserGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<int>("AppGroupId")
                        .HasColumnType("int");

                    b.Property<int>("AppUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AppGroupId");

                    b.HasIndex("AppUserId");

                    b.ToTable("AppUserGroup");
                });

            modelBuilder.Entity("Identity.API.Entities.RefreshToken", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(250)")
                        .HasDefaultValueSql("newid()")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<int>("AppUserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("CreatedByIp")
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<DateTime>("Expires")
                        .HasColumnType("datetime");

                    b.Property<string>("ReplacedByToken")
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Revoked")
                        .HasColumnType("datetime");

                    b.Property<string>("RevokedByIp")
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("varchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(false);

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("Identity.API.Entities.AppGroupPermission", b =>
                {
                    b.HasOne("Identity.API.Entities.AppGroup", "AppGroup")
                        .WithMany("GroupPermissions")
                        .HasForeignKey("AppGroupId")
                        .HasConstraintName("FK_AppGroup_GroupPermission")
                        .IsRequired();

                    b.HasOne("Identity.API.Entities.AppPermission", "Permission")
                        .WithMany("GroupPermissions")
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK_AppPermission_GroupPermission")
                        .IsRequired();
                });

            modelBuilder.Entity("Identity.API.Entities.AppUserGroup", b =>
                {
                    b.HasOne("Identity.API.Entities.AppGroup", "AppGroup")
                        .WithMany("UserGroups")
                        .HasForeignKey("AppGroupId")
                        .HasConstraintName("FK_AppGroup_UserGroup")
                        .IsRequired();

                    b.HasOne("Identity.API.Entities.AppUser", "AppUser")
                        .WithMany("UserGroups")
                        .HasForeignKey("AppUserId")
                        .HasConstraintName("FK_AppUser_UserGroup")
                        .IsRequired();
                });

            modelBuilder.Entity("Identity.API.Entities.RefreshToken", b =>
                {
                    b.HasOne("Identity.API.Entities.AppUser", "AppUser")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("AppUserId")
                        .HasConstraintName("FK_AppUser_RefreshToken")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
