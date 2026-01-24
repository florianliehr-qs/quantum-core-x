#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuantumCore.Game.Persistence.Migrations.Postgresql;

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
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                PlayerId = table.Column<long>(type: "bigint", nullable: false),
                QuestName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                StateIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                IsStarted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                QuestDataId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
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
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                    defaultValueSql: "current_timestamp"),
                QuestDataId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                TriggerAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
