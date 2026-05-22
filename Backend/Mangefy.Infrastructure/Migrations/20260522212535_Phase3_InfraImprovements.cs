using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_InfraImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItemRecipes",
                table: "MenuItemRecipes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MenuItemRecipes");

            migrationBuilder.AddColumn<string>(
                name: "DiscountAmountCurrency",
                table: "Tabs",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceFeeCurrency",
                table: "Tabs",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipCurrency",
                table: "Tabs",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Reservations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql(
                "ALTER TABLE \"OrderItems\" ALTER COLUMN \"Modifiers\" TYPE jsonb USING \"Modifiers\"::jsonb;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItemRecipes",
                table: "MenuItemRecipes",
                columns: new[] { "MenuItemId", "StockItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockItemId",
                table: "StockMovements",
                column: "StockItemId");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Employees_TenantId_Email\" ON \"Employees\" (\"TenantId\", \"Email\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Employees_TenantId_Email\";");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockItemId",
                table: "StockMovements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItemRecipes",
                table: "MenuItemRecipes");

            migrationBuilder.DropColumn(
                name: "DiscountAmountCurrency",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "ServiceFeeCurrency",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "TipCurrency",
                table: "Tabs");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Reservations");

            migrationBuilder.Sql(
                "ALTER TABLE \"OrderItems\" ALTER COLUMN \"Modifiers\" TYPE text USING \"Modifiers\"::text;");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MenuItemRecipes",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItemRecipes",
                table: "MenuItemRecipes",
                columns: new[] { "MenuItemId", "Id" });
        }
    }
}
