using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosquitoApi.Infrastructure.Migrations.AuditDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    changed_fields = table.Column<string>(type: "jsonb", nullable: true),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    action_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.CheckConstraint("ck_audit_logs_action_timestamp_not_future", "action_timestamp <= CURRENT_TIMESTAMP");
                    table.CheckConstraint("ck_audit_logs_create_no_old_values", "(action = 1 AND old_values IS NULL) OR action != 1");
                    table.CheckConstraint("ck_audit_logs_created_at_not_future", "created_at <= CURRENT_TIMESTAMP");
                    table.CheckConstraint("ck_audit_logs_delete_no_new_values", "(action = 3 AND new_values IS NULL) OR action != 3");
                    table.CheckConstraint("ck_audit_logs_valid_action", "action IN (1, 2, 3)");
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action",
                schema: "audit",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_action_timestamp",
                schema: "audit",
                table: "audit_logs",
                column: "action_timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_id",
                schema: "audit",
                table: "audit_logs",
                column: "correlation_id",
                filter: "correlation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_correlation_timestamp",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "correlation_id", "action_timestamp" },
                filter: "correlation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_id",
                schema: "audit",
                table: "audit_logs",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_table_action_timestamp",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "table_name", "action", "action_timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_table_entity",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "table_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_table_name",
                schema: "audit",
                table: "audit_logs",
                column: "table_name");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id",
                schema: "audit",
                table: "audit_logs",
                column: "user_id",
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_timestamp",
                schema: "audit",
                table: "audit_logs",
                columns: new[] { "user_id", "action_timestamp" },
                filter: "user_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "audit");
        }
    }
}
