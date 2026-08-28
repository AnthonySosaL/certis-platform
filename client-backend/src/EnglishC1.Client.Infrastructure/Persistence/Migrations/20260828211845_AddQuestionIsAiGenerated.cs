using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishC1.Client.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionIsAiGenerated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "Questions");
        }
    }
}
