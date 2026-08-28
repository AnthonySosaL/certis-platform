using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishC1.Client.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionPassage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Passage",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passage",
                table: "Questions");
        }
    }
}
