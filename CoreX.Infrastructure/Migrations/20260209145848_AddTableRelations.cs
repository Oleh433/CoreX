using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Clubs_ClubId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Clubs_ClubId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_ClubId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Discounts");

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
                name: "ClubId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clubs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DiscountId",
                table: "Bookings",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Discounts_DiscountId",
                table: "Bookings",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Clubs_ClubId",
                table: "Subscriptions",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Discounts_DiscountId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Clubs_ClubId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_DiscountId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "Bookings");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApplicantId",
                table: "VacancyApplications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClubId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Memberships",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "Discounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_ClubId",
                table: "Discounts",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Clubs_ClubId",
                table: "Discounts",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Clubs_ClubId",
                table: "Subscriptions",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id");
        }
    }
}
