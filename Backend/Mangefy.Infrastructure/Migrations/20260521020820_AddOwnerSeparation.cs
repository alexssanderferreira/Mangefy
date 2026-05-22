using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mangefy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerSeparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Tenants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "MaxEstablishments",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OwnerActivationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerActivationTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            // Cria índice único de Email ANTES do INSERT ON CONFLICT
            migrationBuilder.CreateIndex(
                name: "IX_Owners_Email",
                table: "Owners",
                column: "Email",
                unique: true);

            // ── Migração de dados: employees com IsOwnerRole → tabela Owners ──
            // Lê IsOwner ainda existente em Employees (coluna dropada DEPOIS desta SQL).
            migrationBuilder.Sql(@"
                INSERT INTO ""Owners"" (""Id"", ""Name"", ""Email"", ""PasswordHash"", ""Status"", ""CreatedAt"")
                SELECT
                    gen_random_uuid(),
                    e.""Name"",
                    e.""Email"",
                    e.""PasswordHash"",
                    CASE WHEN e.""Status"" = 'Active' THEN 'Active'
                         WHEN e.""Status"" = 'Inactive' THEN 'Inactive'
                         ELSE 'PendingActivation' END,
                    NOW()
                FROM ""Employees"" e
                WHERE e.""TenantRoleId"" IN (
                    SELECT ""Id"" FROM ""TenantRoles"" WHERE ""IsOwnerRole"" = TRUE
                )
                ON CONFLICT (""Email"") DO NOTHING;

                UPDATE ""Tenants"" t
                SET ""OwnerId"" = o.""Id""
                FROM ""Employees"" e
                INNER JOIN ""Owners"" o ON o.""Email"" = e.""Email""
                WHERE e.""TenantId"" = t.""Id""
                  AND e.""TenantRoleId"" IN (
                      SELECT ""Id"" FROM ""TenantRoles"" WHERE ""IsOwnerRole"" = TRUE
                  );

                UPDATE ""Plans"" SET ""MaxEstablishments"" = 1 WHERE ""MaxEstablishments"" = 0;

                -- Falha explícita se algum Tenant ficou órfão (OwnerId zerado)
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM ""Tenants"" WHERE ""OwnerId"" = '00000000-0000-0000-0000-000000000000') THEN
                        RAISE EXCEPTION 'Migration AddOwnerSeparation: existe(m) Tenant(s) sem Owner correspondente. Verifique os Employees com IsOwnerRole antes de rodar.';
                    END IF;
                END $$;
            ");

            // Agora pode dropar IsOwner em Employees
            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_OwnerId",
                table: "Tenants",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerActivationTokens_OwnerId",
                table: "OwnerActivationTokens",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerActivationTokens_Token",
                table: "OwnerActivationTokens",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Owners_OwnerId",
                table: "Tenants",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OwnerActivationTokens_Owners_OwnerId",
                table: "OwnerActivationTokens",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OwnerActivationTokens_Owners_OwnerId",
                table: "OwnerActivationTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Owners_OwnerId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "OwnerActivationTokens");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_OwnerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxEstablishments",
                table: "Plans");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
