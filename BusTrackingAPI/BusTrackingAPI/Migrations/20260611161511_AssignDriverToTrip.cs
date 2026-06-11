using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusTrackingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AssignDriverToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Trips",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Trips" AS t
                SET "DriverId" = b."DriverId"
                FROM "Buses" AS b
                WHERE t."BusId" = b."Id"
                  AND b."DriverId" IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Users_DriverId",
                table: "Buses");

            migrationBuilder.DropIndex(
                name: "IX_Buses_DriverId",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Buses");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverId",
                table: "Trips",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_DriverId",
                table: "Trips",
                column: "DriverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_DriverId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_DriverId",
                table: "Trips");

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Buses",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Buses" AS b
                SET "DriverId" = latest."DriverId"
                FROM (
                    SELECT DISTINCT ON ("BusId") "BusId", "DriverId"
                    FROM "Trips"
                    WHERE "DriverId" IS NOT NULL
                    ORDER BY "BusId", "ScheduledDeparture" DESC
                ) AS latest
                WHERE b."Id" = latest."BusId";
                """);

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Trips");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_DriverId",
                table: "Buses",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Users_DriverId",
                table: "Buses",
                column: "DriverId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
