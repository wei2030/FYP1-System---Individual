using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FYP1_System___Individual.Migrations
{
    /// <inheritdoc />
    public partial class add_tentativesupervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "Proposals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SupervisorStatus",
                table: "Proposals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TentativeSupervisorId",
                table: "Proposals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_TentativeSupervisorId",
                table: "Proposals",
                column: "TentativeSupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Lecturers_TentativeSupervisorId",
                table: "Proposals",
                column: "TentativeSupervisorId",
                principalTable: "Lecturers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Lecturers_TentativeSupervisorId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_TentativeSupervisorId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SupervisorStatus",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "TentativeSupervisorId",
                table: "Proposals");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "Proposals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
