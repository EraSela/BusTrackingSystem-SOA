using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class TripCenteredScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Buses_BusId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BusId_SeatNumber_ReservationDate",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "BusLocations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Origin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Bus 1");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bus 2");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bus 3");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Bus 4");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Bus 5");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Bus 6");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Bus 7");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Bus 8");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Bus 9");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Bus 10");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 11,
                column: "Name",
                value: "Bus 11");

            migrationBuilder.InsertData(
                table: "Routes",
                columns: new[] { "Id", "Destination", "ExpectedDurationMinutes", "IsActive", "Name", "Origin" },
                values: new object[,]
                {
                    { 1, "Skopje", 180, true, "Struga - Tetovo - Skopje", "Struga" },
                    { 2, "Struga", 180, true, "Skopje - Tetovo - Struga", "Skopje" },
                    { 3, "Struga", 120, true, "Tetovo - Struga", "Tetovo" }
                });

            migrationBuilder.Sql(
                """
                UPDATE "Trips"
                SET "RouteId" = CASE WHEN "Direction" = 1 THEN 2 ELSE 1 END;

                UPDATE "Reservations" AS r
                SET "TripId" = (
                    SELECT t."Id"
                    FROM "Trips" AS t
                    WHERE t."BusId" = r."BusId"
                      AND DATE(t."ScheduledDeparture") = DATE(r."ReservationDate")
                    ORDER BY t."ScheduledDeparture"
                    LIMIT 1
                );

                UPDATE "BusLocations" AS l
                SET "TripId" = (
                    SELECT t."Id"
                    FROM "Trips" AS t
                    WHERE t."BusId" = l."BusId"
                      AND l."Timestamp" BETWEEN
                          COALESCE(t."ActualDeparture", t."ScheduledDeparture" - INTERVAL '1 hour')
                          AND COALESCE(t."ActualArrival", t."ScheduledArrival" + INTERVAL '1 hour')
                    ORDER BY ABS(EXTRACT(EPOCH FROM (l."Timestamp" - t."ScheduledDeparture")))
                    LIMIT 1
                );
                """);

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "BusId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Buses");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_RouteId",
                table: "Trips",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TripId_SeatNumber",
                table: "Reservations",
                columns: new[] { "TripId", "SeatNumber" },
                unique: true,
                filter: "\"TripId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BusLocations_TripId",
                table: "BusLocations",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusLocations_Trips_TripId",
                table: "BusLocations",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Trips_TripId",
                table: "Reservations",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Routes_RouteId",
                table: "Trips",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusLocations_Trips_TripId",
                table: "BusLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Trips_TripId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Routes_RouteId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Trips_RouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TripId_SeatNumber",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_BusLocations_TripId",
                table: "BusLocations");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "BusLocations");

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "Trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusId",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationDate",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Buses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "SIM808_01", "Strugë - Tetovë - Shkup 05:00" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Strugë - Tetovë - Shkup 08:00" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Strugë - Tetovë - Shkup 12:00" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Shkup - Strugë 11:00" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Shkup - Strugë 13:15" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Shkup - Strugë 16:20" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Tetovë - Strugë 12:00" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Tetovë - Strugë 14:15" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Tetovë - Strugë 17:20" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Strugë - Tetovë - Shkup 17:00 Monday/Friday/Sunday" });

            migrationBuilder.UpdateData(
                table: "Buses",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DeviceId", "Name" },
                values: new object[] { "", "Shkup - Strugë 21:00 Monday/Friday/Sunday" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BusId_SeatNumber_ReservationDate",
                table: "Reservations",
                columns: new[] { "BusId", "SeatNumber", "ReservationDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Buses_BusId",
                table: "Reservations",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
