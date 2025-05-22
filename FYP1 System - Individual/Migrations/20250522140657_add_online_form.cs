using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP1_System___Individual.Migrations
{
    /// <inheritdoc />
    public partial class add_online_form : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Proposals",
                newName: "ProposalDocumentPath");

            migrationBuilder.AddColumn<string>(
                name: "OnlineProposalFormPath",
                table: "Proposals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineProposalFormPath",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "ProposalDocumentPath",
                table: "Proposals",
                newName: "FilePath");
        }
    }
}
