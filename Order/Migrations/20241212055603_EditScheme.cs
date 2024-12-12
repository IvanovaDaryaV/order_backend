using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Migrations
{
    /// <inheritdoc />
    public partial class EditScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Contexts_ContextId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ContextId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "privateTasksId",
                table: "ScheduleSharings");

            migrationBuilder.DropColumn(
                name: "ContextId",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Contexts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProjectId",
                table: "Events",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contexts_UserId",
                table: "Contexts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contexts_AspNetUsers_UserId",
                table: "Contexts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Projects_ProjectId",
                table: "Events",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contexts_AspNetUsers_UserId",
                table: "Contexts");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Projects_ProjectId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ProjectId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Contexts_UserId",
                table: "Contexts");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contexts");

            migrationBuilder.AddColumn<int[]>(
                name: "privateTasksId",
                table: "ScheduleSharings",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "ContextId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ContextId",
                table: "Events",
                column: "ContextId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Contexts_ContextId",
                table: "Events",
                column: "ContextId",
                principalTable: "Contexts",
                principalColumn: "Id");
        }
    }
}
