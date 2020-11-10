using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.API.Migrations
{
    public partial class UserPermissionAndGroup_TablesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_AppUser_AppUserId",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(name: "PK_RefreshToken", table: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "Password",
                newName: "PasswordHash",
                table: "AppUser");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshToken",
                unicode: false,
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RevokedByIp",
                table: "RefreshToken",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Revoked",
                table: "RefreshToken",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshToken",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Expires",
                table: "RefreshToken",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByIp",
                table: "RefreshToken",
                unicode: false,
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RefreshToken",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "AppUserId",
                table: "RefreshToken",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
               name: "Id",
               table: "RefreshToken");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "RefreshToken",
                unicode: false,
                maxLength: 250,
                nullable: false,
                defaultValueSql: "newid()");

            migrationBuilder.AddPrimaryKey(name: "PK_RefreshToken", table: "RefreshToken", column: "Id");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "AppUser",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateTimeUtc",
                table: "AppUser",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AppUser",
                unicode: false,
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "AppUser",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "AppUser",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndDateTimeUtc",
                table: "AppUser",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDateTimeUtc",
                table: "AppUser",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppGroup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(unicode: false, maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPermission",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(unicode: false, maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPermission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(unicode: false, maxLength: 250, nullable: false, defaultValueSql: "newid()"),
                    AppUserId = table.Column<int>(nullable: false),
                    AppGroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppGroup_UserGroup",
                        column: x => x.AppGroupId,
                        principalTable: "AppGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUser_UserGroup",
                        column: x => x.AppUserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppGroupPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(unicode: false, maxLength: 250, nullable: false, defaultValueSql: "newid()"),
                    PermissionId = table.Column<int>(nullable: false),
                    AppGroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGroupPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppGroup_GroupPermission",
                        column: x => x.AppGroupId,
                        principalTable: "AppGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppPermission_GroupPermission",
                        column: x => x.PermissionId,
                        principalTable: "AppPermission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppGroupPermission_AppGroupId",
                table: "AppGroupPermission",
                column: "AppGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGroupPermission_PermissionId",
                table: "AppGroupPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserGroup_AppGroupId",
                table: "AppUserGroup",
                column: "AppGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserGroup_AppUserId",
                table: "AppUserGroup",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUser_RefreshToken",
                table: "RefreshToken",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUser_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.DropTable(
                name: "AppGroupPermission");

            migrationBuilder.DropTable(
                name: "AppUserGroup");

            migrationBuilder.DropTable(
                name: "AppPermission");

            migrationBuilder.DropTable(
                name: "AppGroup");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "CreatedDateTimeUtc",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "LockoutEndDateTimeUtc",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "ModifiedDateTimeUtc",
                table: "AppUser");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                newName: "Password",
                table: "AppUser");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "RevokedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Revoked",
                table: "RefreshToken",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Expires",
                table: "RefreshToken",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RefreshToken",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<int>(
                name: "AppUserId",
                table: "RefreshToken",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "RefreshToken",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldUnicode: false,
                oldMaxLength: 250,
                oldDefaultValueSql: "newid()")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_AppUser_AppUserId",
                table: "RefreshToken",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
