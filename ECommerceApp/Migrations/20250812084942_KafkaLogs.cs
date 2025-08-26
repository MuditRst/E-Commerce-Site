using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Migrations
{
    /// <inheritdoc />
    public partial class KafkaLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orderStatusHistories_Orders_OrderID",
                table: "orderStatusHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_orderStatusHistories",
                table: "orderStatusHistories");

            migrationBuilder.RenameTable(
                name: "orderStatusHistories",
                newName: "OrderStatusHistories");

            migrationBuilder.RenameIndex(
                name: "IX_orderStatusHistories_OrderID",
                table: "OrderStatusHistories",
                newName: "IX_OrderStatusHistories_OrderID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderStatusHistories",
                table: "OrderStatusHistories",
                column: "HistoryID");

            migrationBuilder.CreateTable(
                name: "KafkaLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KafkaLogs", x => x.LogID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderID",
                table: "OrderStatusHistories",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderID",
                table: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "KafkaLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderStatusHistories",
                table: "OrderStatusHistories");

            migrationBuilder.RenameTable(
                name: "OrderStatusHistories",
                newName: "orderStatusHistories");

            migrationBuilder.RenameIndex(
                name: "IX_OrderStatusHistories_OrderID",
                table: "orderStatusHistories",
                newName: "IX_orderStatusHistories_OrderID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_orderStatusHistories",
                table: "orderStatusHistories",
                column: "HistoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_orderStatusHistories_Orders_OrderID",
                table: "orderStatusHistories",
                column: "OrderID",
                principalTable: "Orders",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
