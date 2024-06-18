using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CannoliSaveStates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannoliSaveStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroceryOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFulfilled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroceryOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CannoliRoutes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CallbackType = table.Column<string>(type: "TEXT", nullable: false),
                    CallbackMethod = table.Column<string>(type: "TEXT", nullable: false),
                    StateId = table.Column<string>(type: "TEXT", nullable: false),
                    IsSynchronous = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeferred = table.Column<bool>(type: "INTEGER", nullable: false),
                    StateIdToBeDeleted = table.Column<string>(type: "TEXT", nullable: true),
                    Parameter1 = table.Column<string>(type: "TEXT", nullable: true),
                    Parameter2 = table.Column<string>(type: "TEXT", nullable: true),
                    Parameter3 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannoliRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CannoliRoutes_CannoliSaveStates_StateId",
                        column: x => x.StateId,
                        principalTable: "CannoliSaveStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroceryOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroceryOrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    FoodItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroceryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroceryOrderItems_FoodItems_FoodItemId",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroceryOrderItems_GroceryOrders_GroceryOrderId",
                        column: x => x.GroceryOrderId,
                        principalTable: "GroceryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CannoliRoutes_Id_Type",
                table: "CannoliRoutes",
                columns: new[] { "Id", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_CannoliRoutes_StateId",
                table: "CannoliRoutes",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CannoliSaveStates_ExpiresOn",
                table: "CannoliSaveStates",
                column: "ExpiresOn");

            migrationBuilder.CreateIndex(
                name: "IX_GroceryOrderItems_FoodItemId",
                table: "GroceryOrderItems",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GroceryOrderItems_GroceryOrderId",
                table: "GroceryOrderItems",
                column: "GroceryOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CannoliRoutes");

            migrationBuilder.DropTable(
                name: "GroceryOrderItems");

            migrationBuilder.DropTable(
                name: "CannoliSaveStates");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "GroceryOrders");
        }
    }
}
