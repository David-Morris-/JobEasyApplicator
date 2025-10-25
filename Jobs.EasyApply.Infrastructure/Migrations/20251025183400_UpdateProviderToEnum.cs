using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.EasyApply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProviderToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, update existing records to use integer values
            // LinkedIn = 0 (default enum value)
            migrationBuilder.Sql("UPDATE AppliedJobs SET Provider = '0' WHERE Provider = 'LinkedIn' OR Provider = ''");

            // Change column type from TEXT to INTEGER
            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "AppliedJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Change column type back from INTEGER to TEXT
            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "AppliedJobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Convert integer values back to string
            migrationBuilder.Sql("UPDATE AppliedJobs SET Provider = 'LinkedIn' WHERE Provider = '0'");
        }
    }
}
