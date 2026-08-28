using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishC1.Client.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionAudioUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Questions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Questions");
        }
    }
}
