using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharpMind.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCertificateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalTestScore",
                table: "Certificates",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MentorName",
                table: "Certificates",
                type: "TEXT",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalTestScore",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "MentorName",
                table: "Certificates");
        }
    }
}
