using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlanMaxEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxEstablishments",
                table: "Plans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxEstablishments",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
