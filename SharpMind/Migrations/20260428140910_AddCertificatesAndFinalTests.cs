using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharpMind.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificatesAndFinalTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsFinalTest",
                table: "Tests",
                newName: "IsFinal");

            migrationBuilder.RenameColumn(
                name: "StudentFullName",
                table: "Certificates",
                newName: "FullName");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId_StudentId",
                table: "Certificates",
                columns: new[] { "CourseId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UniqueNumber",
                table: "Certificates",
                column: "UniqueNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Certificates_CourseId_StudentId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_UniqueNumber",
                table: "Certificates");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Certificates",
                newName: "StudentFullName");

            migrationBuilder.RenameColumn(
                name: "IsFinal",
                table: "Tests",
                newName: "IsFinalTest");
        }
    }
}
