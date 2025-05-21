using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP1_System___Individual.Migrations
{
    /// <inheritdoc />
    public partial class add_ps_reset_token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Students",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Students");
        }
    }
}
