using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Migrations
{
    /// <inheritdoc />
    public partial class migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            {
                migrationBuilder.AddColumn<string>(
                    name: "theme",
                    table: "users",
                    type: "text",
                    nullable: true
                    );

                migrationBuilder.AddColumn<string>(
                    name: "language",
                    table: "users",
                    type: "text",
                    nullable: true
                    );

                migrationBuilder.AddColumn<int>(
                    name: "complexity",
                    table: "tasks",
                    type: "integer",
                    nullable: true);

                migrationBuilder.AddColumn<DateTime>(
                    name: "date_created",
                    table: "tasks",
                    type: "timestamp without time zone",
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "type",
                    table: "tasks",
                    type: "text",
                    nullable: true);
            }
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "complexity",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "date_created",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "type",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "theme",
                table: "users");

            migrationBuilder.DropColumn(
                name: "language",
                table: "users");

        }
    }
}
