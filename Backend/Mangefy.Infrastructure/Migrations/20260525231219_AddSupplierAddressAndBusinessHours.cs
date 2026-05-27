using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierAddressAndBusinessHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressBairro",
                table: "PlatformSuppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCep",
                table: "PlatformSuppliers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCidade",
                table: "PlatformSuppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressComplemento",
                table: "PlatformSuppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLogradouro",
                table: "PlatformSuppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumero",
                table: "PlatformSuppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressUf",
                table: "PlatformSuppliers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "PlatformSuppliers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressBairro",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressCep",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressCidade",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressComplemento",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressLogradouro",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressNumero",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "AddressUf",
                table: "PlatformSuppliers");

            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "PlatformSuppliers");
        }
    }
}
