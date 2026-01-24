#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Mysql;

/// <inheritdoc />
public partial class AddQuestEntities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "QuestData",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                PlayerId = table.Column<uint>(type: "int unsigned", nullable: false),
                QuestName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                StateIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsStarted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuestData", x => x.Id);
                table.ForeignKey(
                    name: "FK_QuestData_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QuestFlags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                QuestDataId = table.Column<Guid>(type: "char(36)", nullable: false),
                Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                Value = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuestFlags", x => x.Id);
                table.ForeignKey(
                    name: "FK_QuestFlags_QuestData_QuestDataId",
                    column: x => x.QuestDataId,
                    principalTable: "QuestData",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "QuestTimers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false,
                    defaultValueSql: "(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))"),
                QuestDataId = table.Column<Guid>(type: "char(36)", nullable: false),
                Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                TriggerAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                IsProcessed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuestTimers", x => x.Id);
                table.ForeignKey(
                    name: "FK_QuestTimers_QuestData_QuestDataId",
                    column: x => x.QuestDataId,
                    principalTable: "QuestData",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_QuestData_PlayerId_QuestName",
            table: "QuestData",
            columns: new[] { "PlayerId", "QuestName" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_QuestFlags_QuestDataId_Name",
            table: "QuestFlags",
            columns: new[] { "QuestDataId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_QuestTimers_QuestDataId_Name",
            table: "QuestTimers",
            columns: new[] { "QuestDataId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_QuestTimers_TriggerAt_IsProcessed",
            table: "QuestTimers",
            columns: new[] { "TriggerAt", "IsProcessed" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "QuestTimers");

        migrationBuilder.DropTable(
            name: "QuestFlags");

        migrationBuilder.DropTable(
            name: "QuestData");
    }
}
