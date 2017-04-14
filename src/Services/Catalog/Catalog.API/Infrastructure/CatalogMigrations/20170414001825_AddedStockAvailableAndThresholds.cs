using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Catalog.API.Infrastructure.CatalogMigrations
{
    public partial class AddedStockAvailableAndThresholds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Units",
                table: "Catalog",
                newName: "RestockThreshold");

            migrationBuilder.AddColumn<int>(
                name: "AvailableStock",
                table: "Catalog",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxStockThreshold",
                table: "Catalog",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OnReorder",
                table: "Catalog",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableStock",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "MaxStockThreshold",
                table: "Catalog");

            migrationBuilder.DropColumn(
                name: "OnReorder",
                table: "Catalog");

            migrationBuilder.RenameColumn(
                name: "RestockThreshold",
                table: "Catalog",
                newName: "Units");
        }
    }
}
