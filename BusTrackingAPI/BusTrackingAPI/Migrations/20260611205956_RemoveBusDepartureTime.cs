using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBusDepartureTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "Buses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DepartureTime",
                table: "Buses",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 1,
                column: "DepartureTime",
                value: new TimeSpan(0, 5, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 2,
                column: "DepartureTime",
                value: new TimeSpan(0, 8, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 3,
                column: "DepartureTime",
                value: new TimeSpan(0, 12, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 4,
                column: "DepartureTime",
                value: new TimeSpan(0, 11, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 5,
                column: "DepartureTime",
                value: new TimeSpan(0, 13, 15, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 6,
                column: "DepartureTime",
                value: new TimeSpan(0, 16, 20, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 7,
                column: "DepartureTime",
                value: new TimeSpan(0, 12, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 8,
                column: "DepartureTime",
                value: new TimeSpan(0, 14, 15, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 9,
                column: "DepartureTime",
                value: new TimeSpan(0, 17, 20, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 10,
                column: "DepartureTime",
                value: new TimeSpan(0, 17, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 11,
                column: "DepartureTime",
                value: new TimeSpan(0, 21, 0, 0, 0));
        }
    }
}
