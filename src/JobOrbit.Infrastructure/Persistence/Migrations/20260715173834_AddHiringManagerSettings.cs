using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobOrbit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringManagerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CandidateReviewNotifications",
                table: "HiringManagerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DecisionNotifications",
                table: "HiringManagerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotifications",
                table: "HiringManagerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EvaluationNotifications",
                table: "HiringManagerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "InterviewNotifications",
                table: "HiringManagerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "HiringManagerProfiles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "Hiring Manager");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "HiringManagerProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidateReviewNotifications",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "DecisionNotifications",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "EmailNotifications",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "EvaluationNotifications",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "InterviewNotifications",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "HiringManagerProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "HiringManagerProfiles");
        }
    }
}
