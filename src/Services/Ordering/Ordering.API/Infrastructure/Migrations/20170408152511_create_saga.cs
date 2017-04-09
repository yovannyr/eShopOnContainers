using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Ordering.API.Migrations
{
    public partial class create_saga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSaga",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Cancelled = table.Column<bool>(nullable: false),
                    Completed = table.Column<bool>(nullable: false),
                    CorrelationId = table.Column<int>(nullable: false),
                    IsPaymentDone = table.Column<bool>(nullable: false),
                    IsStockProvided = table.Column<bool>(nullable: false),
                    Originator = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSaga", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSaga");
        }
    }
}
