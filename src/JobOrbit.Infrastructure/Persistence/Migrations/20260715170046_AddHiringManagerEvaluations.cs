using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobOrbit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringManagerEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_JobApplicationId",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_OverallScore",
                table: "CandidateEvaluations");

            migrationBuilder.AlterColumn<int>(
                name: "RecruiterProfileId",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CommunicationScore",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CultureFitScore",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EvaluatorUserId",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceScore",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Recommendation",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicalScore",
                table: "CandidateEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HiringManagerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringManagerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringManagerProfiles_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringManagerProfiles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringManagerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_EvaluatorUserId",
                table: "CandidateEvaluations",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_JobApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations",
                columns: new[] { "JobApplicationId", "EvaluatorUserId" },
                unique: true,
                filter: "[EvaluatorUserId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_CommunicationScore",
                table: "CandidateEvaluations",
                sql: "[CommunicationScore] IS NULL OR ([CommunicationScore] >= 1 AND [CommunicationScore] <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_CultureFitScore",
                table: "CandidateEvaluations",
                sql: "[CultureFitScore] IS NULL OR ([CultureFitScore] >= 1 AND [CultureFitScore] <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_ExperienceScore",
                table: "CandidateEvaluations",
                sql: "[ExperienceScore] IS NULL OR ([ExperienceScore] >= 1 AND [ExperienceScore] <= 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_OverallScore",
                table: "CandidateEvaluations",
                sql: "[OverallScore] >= 0 AND [OverallScore] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_TechnicalScore",
                table: "CandidateEvaluations",
                sql: "[TechnicalScore] IS NULL OR ([TechnicalScore] >= 1 AND [TechnicalScore] <= 10)");

            migrationBuilder.CreateIndex(
                name: "IX_HiringManagerProfiles_DepartmentId",
                table: "HiringManagerProfiles",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringManagerProfiles_OrganizationId",
                table: "HiringManagerProfiles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringManagerProfiles_UserId",
                table: "HiringManagerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateEvaluations_Users_EvaluatorUserId",
                table: "CandidateEvaluations",
                column: "EvaluatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateEvaluations_Users_EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropTable(
                name: "HiringManagerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_JobApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_CommunicationScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_CultureFitScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_ExperienceScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_OverallScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CandidateEvaluations_TechnicalScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "CommunicationScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "CultureFitScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "ExperienceScore",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "TechnicalScore",
                table: "CandidateEvaluations");

            migrationBuilder.AlterColumn<int>(
                name: "RecruiterProfileId",
                table: "CandidateEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_JobApplicationId",
                table: "CandidateEvaluations",
                column: "JobApplicationId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CandidateEvaluations_OverallScore",
                table: "CandidateEvaluations",
                sql: "[OverallScore] >= 0 AND [OverallScore] <= 5");
        }
    }
}
