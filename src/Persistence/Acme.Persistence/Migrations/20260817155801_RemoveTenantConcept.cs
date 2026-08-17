using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acme.Persistence.Migrations
{
    /// <summary>
    /// Removes the tenant concept (ADR-066): one database per customer, so a
    /// tenant column is a constant repeated on every row.
    /// </summary>
    /// <remarks>
    /// <b>This migration is only valid against a database holding exactly one
    /// customer.</b> The final step narrows the documents unique index from
    /// <c>(TenantId, Name)</c> to <c>(Name)</c>, and two tenants sharing a
    /// document name is legal under the old index — so on a shared database
    /// this fails with a duplicate-key error, which is the correct outcome.
    /// Split the data per customer first; see ADR-066 decision 6.
    /// <para>
    /// <b>Down does not round-trip.</b> It restores the columns, the table and
    /// the indexes, but not the platform-administrator rows <c>Up</c> deletes
    /// — those are gone, and reversing this migration leaves a database that
    /// is structurally correct and short a few users.
    /// </para>
    /// </remarks>
    public partial class RemoveTenantConcept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Platform administrators, before anything else touches Users.
            //
            // Removing UserRole.PlatformAdministrator (= 1) is a data change as
            // much as a code one: a row left holding it materialises into an
            // undefined enum, and the account reaches the worst possible state
            // — sign-in succeeds and issues a token reading "acme:role": "1",
            // then every authenticated request 401s on Enum.IsDefined. It can
            // authenticate and do nothing.
            //
            // Deleting is right rather than merely convenient. That role named
            // someone who operated Acme *across* tenants (ADR-033 rule 1), and
            // under ADR-066 a database belongs to exactly one customer — so an
            // operator of the old shared system has no place in it. Anyone who
            // still needs access is invited as an ordinary Administrator.
            //
            // The five person-scoped satellites cascade (ON DELETE CASCADE on
            // every FK to Users), so credentials, sessions, refresh tokens,
            // invitations and password resets go with them.
            migrationBuilder.Sql("""DELETE FROM "Users" WHERE "Role" = 1;""");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TenantId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TenantId_Name",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Name",
                table: "Documents",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_Name",
                table: "Documents");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Documents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TenantId",
                table: "Documents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TenantId_Name",
                table: "Documents",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name");
        }
    }
}
