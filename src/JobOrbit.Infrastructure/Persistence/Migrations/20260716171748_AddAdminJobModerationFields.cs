using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobOrbit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminJobModerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "JobPostings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "LKR");

            migrationBuilder.AddColumn<string>(
                name: "ExperienceLevel",
                table: "JobPostings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "JobPostings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VacancyCount",
                table: "JobPostings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "WorkplaceType",
                table: "JobPostings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "VacancyCount",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "WorkplaceType",
                table: "JobPostings");
        }
    }
}
