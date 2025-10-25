using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.EasyApply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderToAppliedJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "AppliedJobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "AppliedJobs");
        }
    }
}
