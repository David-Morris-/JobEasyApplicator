using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jobs.EasyApply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToAppliedJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "AppliedJobs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppliedJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppliedJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppliedJobs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppliedJobs");

            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "AppliedJobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
