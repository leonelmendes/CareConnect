using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAvatarUtente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Patients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Patients");
        }
    }
}
