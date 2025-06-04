using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Order.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedule_sharing",
                columns: table => new
                {
                    share_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    public_link_token = table.Column<string>(type: "text", nullable: false),
                    private_events_id = table.Column<int[]>(type: "integer[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_sharing", x => x.share_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    theme = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "contexts",
                columns: table => new
                {
                    context_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    place = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contexts", x => x.context_id);
                    table.ForeignKey(
                        name: "fk_contexts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    context_id = table.Column<int>(type: "integer", nullable: true),
                    hard_deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    soft_deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    links = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.project_id);
                    table.ForeignKey(
                        name: "fk_projects_contexts_context_id",
                        column: x => x.context_id,
                        principalTable: "contexts",
                        principalColumn: "context_id");
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    was_attended = table.Column<bool>(type: "boolean", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    period_start = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    period_end = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_events_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "project_id");
                    table.ForeignKey(
                        name: "fk_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    note_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    last_edited = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tag = table.Column<string>(type: "text", nullable: true),
                    is_done = table.Column<bool>(type: "boolean", nullable: true),
                    project_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.note_id);
                    table.ForeignKey(
                        name: "fk_notes_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "project_id");
                    table.ForeignKey(
                        name: "fk_notes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects_users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects_users", x => new { x.project_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_projects_users_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_projects_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    hard_deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    soft_deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    date_done = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    context_id = table.Column<int>(type: "integer", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    complexity = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<int>(type: "integer", nullable: true),
                    calendar_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: true),
                    project_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.task_id);
                    table.ForeignKey(
                        name: "fk_tasks_contexts_context_id",
                        column: x => x.context_id,
                        principalTable: "contexts",
                        principalColumn: "context_id");
                    table.ForeignKey(
                        name: "fk_tasks_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id");
                    table.ForeignKey(
                        name: "fk_tasks_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "project_id");
                    table.ForeignKey(
                        name: "fk_tasks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contexts_user_id",
                table: "contexts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_project_id",
                table: "events",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_user_id",
                table: "events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_project_id",
                table: "notes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_user_id",
                table: "notes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_context_id",
                table: "projects",
                column: "context_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_users_user_id",
                table: "projects_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_context_id",
                table: "tasks",
                column: "context_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_event_id",
                table: "tasks",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_project_id",
                table: "tasks",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_user_id",
                table: "tasks",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "projects_users");

            migrationBuilder.DropTable(
                name: "schedule_sharing");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "contexts");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
