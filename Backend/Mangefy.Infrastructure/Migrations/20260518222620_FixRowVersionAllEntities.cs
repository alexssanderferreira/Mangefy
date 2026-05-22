using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop physical bytea RowVersion columns — xmin is a PostgreSQL system column, no physical column needed
            migrationBuilder.DropColumn(name: "RowVersion", table: "Tabs");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Stocks");
            migrationBuilder.DropColumn(name: "RowVersion", table: "CashRegisters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(name: "RowVersion", table: "Tabs",         type: "bytea", rowVersion: true, nullable: false, defaultValue: new byte[0]);
            migrationBuilder.AddColumn<byte[]>(name: "RowVersion", table: "Stocks",       type: "bytea", rowVersion: true, nullable: false, defaultValue: new byte[0]);
            migrationBuilder.AddColumn<byte[]>(name: "RowVersion", table: "CashRegisters",type: "bytea", rowVersion: true, nullable: false, defaultValue: new byte[0]);
        }
    }
}
