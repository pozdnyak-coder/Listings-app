using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListingsApi.Data.Migrations;

/// <summary>Начальная схема базы: районы и объявления + стартовые данные.</summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261010150000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Districts",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Districts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Listings",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Price = table.Column<double>(type: "REAL", nullable: false),
                Rooms = table.Column<int>(type: "INTEGER", nullable: false),
                Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                DistrictId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Listings", x => x.Id);
                table.ForeignKey(
                    name: "FK_Listings_Districts_DistrictId",
                    column: x => x.DistrictId,
                    principalTable: "Districts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Стартовые данные обычным SQL: у этой миграции нет встроенной модели,
        // поэтому InsertData не смог бы определить типы столбцов.
        migrationBuilder.Sql(
            """
            INSERT INTO "Districts" ("Id", "Name") VALUES
                (1, 'Чиланзар'),
                (2, 'Юнусабад'),
                (3, 'Мирзо-Улугбек');
            """);

        migrationBuilder.Sql(
            """
            INSERT INTO "Listings" ("Id", "Address", "CreatedAt", "DistrictId", "Price", "Rooms", "Title") VALUES
                (1, 'Чиланзар, 12 кв., д. 4', '2026-09-01 09:00:00', 1, 48000.0, 2, '2-комн. рядом с метро Чиланзар'),
                (2, NULL, '2026-09-05 10:30:00', 2, 71000.0, 3, '3-комн. с ремонтом'),
                (3, NULL, '2026-09-08 14:00:00', 3, 32000.0, 1, '1-комн. студия'),
                (4, 'Юнусабад, 4 кв.', '2026-09-12 08:15:00', 2, 95000.0, 4, '4-комн. с видом на парк'),
                (5, NULL, '2026-09-15 11:00:00', 1, 52000.0, 2, '2-комн. после ремонта, тихий двор'),
                (6, NULL, '2026-09-18 16:45:00', 2, 36000.0, 1, '1-комн. у ТРЦ'),
                (7, NULL, '2026-09-20 09:30:00', 3, 66000.0, 3, '3-комн. семейная'),
                (8, NULL, '2026-09-24 12:00:00', 1, 39000.0, 2, '2-комн. срочно, торг');
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Districts_Name",
            table: "Districts",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Listings_DistrictId",
            table: "Listings",
            column: "DistrictId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Listings");
        migrationBuilder.DropTable(name: "Districts");
    }
}
