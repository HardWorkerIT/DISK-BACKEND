using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using App.Infrastructure.Data;

#nullable disable

namespace App.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261102000000_AddIdentityConstraints")]
public partial class AddIdentityConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "UX_users_active_login",
            table: "users",
            column: "login",
            unique: true,
            filter: "deleted_at IS NULL");

        migrationBuilder.CreateIndex(
            name: "UX_users_active_email",
            table: "users",
            column: "email",
            unique: true,
            filter: "deleted_at IS NULL AND email IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "UX_tokens_token",
            table: "tokens",
            column: "token",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "UX_users_active_login", table: "users");
        migrationBuilder.DropIndex(name: "UX_users_active_email", table: "users");
        migrationBuilder.DropIndex(name: "UX_tokens_token", table: "tokens");
    }
}
