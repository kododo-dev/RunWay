using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kododo.RunWay.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "runway");

            migrationBuilder.CreateTable(
                name: "jobs_audit",
                schema: "runway",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_id = table.Column<long>(type: "bigint", nullable: false),
                    time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs_audit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "runners",
                schema: "runway",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "runner_job_types",
                schema: "runway",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    job_type = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runner_job_types", x => new { x.id, x.job_type });
                    table.ForeignKey(
                        name: "FK_runner_job_types_runners_id",
                        column: x => x.id,
                        principalSchema: "runway",
                        principalTable: "runners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                schema: "runway",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    version = table.Column<int>(type: "int", nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    runner_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    retries_count = table.Column<int>(type: "int", nullable: false),
                    recurrence_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_runners_runner_id",
                        column: x => x.runner_id,
                        principalSchema: "runway",
                        principalTable: "runners",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "recurrences",
                schema: "runway",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    version = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrences", x => x.id);
                    table.ForeignKey(
                        name: "FK_recurrences_jobs_job_id",
                        column: x => x.job_id,
                        principalSchema: "runway",
                        principalTable: "jobs",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_recurrence_id",
                schema: "runway",
                table: "jobs",
                column: "recurrence_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_runner_id",
                schema: "runway",
                table: "jobs",
                column: "runner_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_scheduled_at",
                schema: "runway",
                table: "jobs",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_status",
                schema: "runway",
                table: "jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_status_scheduled_at",
                schema: "runway",
                table: "jobs",
                columns: new[] { "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_audit_job_id_time",
                schema: "runway",
                table: "jobs_audit",
                columns: new[] { "job_id", "time" });

            migrationBuilder.CreateIndex(
                name: "IX_recurrences_job_id",
                schema: "runway",
                table: "recurrences",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurrences_key",
                schema: "runway",
                table: "recurrences",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runner_job_types_job_type",
                schema: "runway",
                table: "runner_job_types",
                column: "job_type");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_recurrences_recurrence_id",
                schema: "runway",
                table: "jobs",
                column: "recurrence_id",
                principalSchema: "runway",
                principalTable: "recurrences",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_recurrences_recurrence_id",
                schema: "runway",
                table: "jobs");

            migrationBuilder.DropTable(
                name: "jobs_audit",
                schema: "runway");

            migrationBuilder.DropTable(
                name: "runner_job_types",
                schema: "runway");

            migrationBuilder.DropTable(
                name: "recurrences",
                schema: "runway");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "runway");

            migrationBuilder.DropTable(
                name: "runners",
                schema: "runway");
        }
    }
}
