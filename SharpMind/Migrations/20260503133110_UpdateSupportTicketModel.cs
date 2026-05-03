using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharpMind.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportTicketModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "ResponseAt",
                table: "SupportTickets",
                newName: "ReplyDate");

            migrationBuilder.RenameColumn(
                name: "IsResolved",
                table: "SupportTickets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "AdminResponse",
                table: "SupportTickets",
                newName: "AdminReply");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "SupportTickets",
                newName: "IsResolved");

            migrationBuilder.RenameColumn(
                name: "ReplyDate",
                table: "SupportTickets",
                newName: "ResponseAt");

            migrationBuilder.RenameColumn(
                name: "AdminReply",
                table: "SupportTickets",
                newName: "AdminResponse");

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                table: "SupportTickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
