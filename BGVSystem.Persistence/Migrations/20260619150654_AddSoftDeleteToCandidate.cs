using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BGVSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Candidates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Candidates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Candidates");
        }
    }
}
