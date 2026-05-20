using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LowPriorityFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicantId",
                table: "VacancyApplications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Drop any orphan bookings that have no Subscription before making the FK column NOT NULL.
            migrationBuilder.Sql("DELETE FROM [Bookings] WHERE [SubscriptionId] IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicantId",
                table: "VacancyApplications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SubscriptionId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
