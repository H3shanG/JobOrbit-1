using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobOrbit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CandidateStatusNotifications",
                table: "RecruiterProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotifications",
                table: "RecruiterProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "InterviewNotifications",
                table: "RecruiterProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "JobApplicationNotifications",
                table: "RecruiterProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateStatusNotifications",
                table: "RecruiterProfiles");

            migrationBuilder.DropColumn(
                name: "EmailNotifications",
                table: "RecruiterProfiles");

            migrationBuilder.DropColumn(
                name: "InterviewNotifications",
                table: "RecruiterProfiles");

            migrationBuilder.DropColumn(
                name: "JobApplicationNotifications",
                table: "RecruiterProfiles");
        }
    }
}
