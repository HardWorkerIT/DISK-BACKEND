using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using App.Infrastructure.Data;

#nullable disable

namespace App.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261103000000_AddFileAndSessionQueryIndexes")]
public partial class AddFileAndSessionQueryIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_files_folder_deleted_at",
            table: "files",
            columns: new[] { "folder_id", "deleted_at" });

        migrationBuilder.CreateIndex(
            name: "IX_folders_parent_deleted_at",
            table: "folders",
            columns: new[] { "parent_id", "deleted_at" });

        migrationBuilder.CreateIndex(
            name: "IX_tokens_user_expired_at",
            table: "tokens",
            columns: new[] { "user_id", "expired_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_files_folder_deleted_at", table: "files");
        migrationBuilder.DropIndex(name: "IX_folders_parent_deleted_at", table: "folders");
        migrationBuilder.DropIndex(name: "IX_tokens_user_expired_at", table: "tokens");
    }
}
