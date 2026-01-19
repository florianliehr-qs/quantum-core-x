using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumCore.Game.Persistence.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddQuestPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerQuests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    QuestId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "start"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "current_timestamp"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    QuestDataJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerQuests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerQuests_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_IsCompleted",
                table: "PlayerQuests",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_PlayerId",
                table: "PlayerQuests",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_PlayerId_QuestId",
                table: "PlayerQuests",
                columns: new[] { "PlayerId", "QuestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuests_QuestId",
                table: "PlayerQuests",
                column: "QuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerQuests");
        }
    }
}
