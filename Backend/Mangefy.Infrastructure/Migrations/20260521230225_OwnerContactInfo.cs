using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OwnerContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_Bairro",
                table: "Owners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Cep",
                table: "Owners",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Cidade",
                table: "Owners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Complemento",
                table: "Owners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Logradouro",
                table: "Owners",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Numero",
                table: "Owners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Uf",
                table: "Owners",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Owners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "Owners",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Owners",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Owners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // FKs FK_OwnerActivationTokens_Owners_OwnerId e FK_Tenants_Owners_OwnerId
            // já foram criadas pela migration AddOwnerSeparation (manualmente editada).
            // EF não detecta isso porque o snapshot anterior não as registrava.
            // Não adicionamos aqui para evitar erro 42710 (constraint duplicada).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Bairro",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Cep",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Cidade",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Complemento",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Logradouro",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Numero",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Address_Uf",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Owners");
        }
    }
}
