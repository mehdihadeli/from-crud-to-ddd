using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCharging.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capacity_in_amps = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "charge_stations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_charge_stations", x => x.id);
                    table.ForeignKey(
                        name: "fk_charge_stations_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "connectors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    charge_station_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_current_in_amps = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connectors", x => new { x.charge_station_id, x.id });
                    table.ForeignKey(
                        name: "fk_connectors_charge_stations_charge_station_id",
                        column: x => x.charge_station_id,
                        principalTable: "charge_stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_charge_stations_group_id",
                table: "charge_stations",
                column: "group_id"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "connectors");

            migrationBuilder.DropTable(name: "charge_stations");

            migrationBuilder.DropTable(name: "groups");
        }
    }
}
