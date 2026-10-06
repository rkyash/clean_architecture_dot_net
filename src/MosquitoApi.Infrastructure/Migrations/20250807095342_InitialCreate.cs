using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosquitoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mosquito");

            migrationBuilder.CreateTable(
                name: "mosquitoes",
                schema: "mosquito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    species = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<int>(type: "integer", nullable: false),
                    length_mm = table.Column<double>(type: "numeric(6,2)", nullable: false),
                    weight_mg = table.Column<double>(type: "numeric(8,3)", nullable: true),
                    color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "jsonb", nullable: false),
                    observed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    temperature_celsius = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    humidity_percent = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_disease_vector = table.Column<bool>(type: "boolean", nullable: false),
                    disease_transmission = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    modified_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mosquitoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "mosquito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    last_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    father_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    created_by_id = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    password = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    dob = table.Column<long>(type: "bigint", nullable: true),
                    mobile = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    role_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    failed_password_attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    reset_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reset_id_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    username = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gender_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_login_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_password_change_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_verification_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lockout_end_date_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                schema: "mosquito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    revoked_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason_revoked = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_by_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    revoked_by_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by_user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_tokens_users_UserId1",
                        column: x => x.UserId1,
                        principalSchema: "mosquito",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "mosquito",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_created_at",
                schema: "mosquito",
                table: "mosquitoes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_created_by",
                schema: "mosquito",
                table: "mosquitoes",
                column: "created_by",
                filter: "created_by IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_disease_vector",
                schema: "mosquito",
                table: "mosquitoes",
                column: "is_disease_vector");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_id_active",
                schema: "mosquito",
                table: "mosquitoes",
                column: "id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_observed_at",
                schema: "mosquito",
                table: "mosquitoes",
                column: "observed_at");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_observed_species",
                schema: "mosquito",
                table: "mosquitoes",
                columns: new[] { "observed_at", "species" });

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_size",
                schema: "mosquito",
                table: "mosquitoes",
                column: "size");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_species",
                schema: "mosquito",
                table: "mosquitoes",
                column: "species");

            migrationBuilder.CreateIndex(
                name: "ix_mosquitoes_species_size",
                schema: "mosquito",
                table: "mosquitoes",
                columns: new[] { "species", "size" });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_active_expiration",
                schema: "mosquito",
                table: "tokens",
                columns: new[] { "is_active", "expiration_date" });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_cleanup",
                schema: "mosquito",
                table: "tokens",
                columns: new[] { "created_date", "revoked_date" },
                filter: "revoked_date IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_created_by_ip",
                schema: "mosquito",
                table: "tokens",
                column: "created_by_ip_address",
                filter: "created_by_ip_address IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_created_date",
                schema: "mosquito",
                table: "tokens",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_expiry_cleanup",
                schema: "mosquito",
                table: "tokens",
                column: "expiration_date");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_id_active",
                schema: "mosquito",
                table: "tokens",
                column: "id",
                filter: "is_active = true AND revoked_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_is_active",
                schema: "mosquito",
                table: "tokens",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_refresh_token_unique",
                schema: "mosquito",
                table: "tokens",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tokens_user_active",
                schema: "mosquito",
                table: "tokens",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_user_id",
                schema: "mosquito",
                table: "tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tokens_user_revoked",
                schema: "mosquito",
                table: "tokens",
                columns: new[] { "user_id", "revoked_date" });

            migrationBuilder.CreateIndex(
                name: "IX_tokens_UserId1",
                schema: "mosquito",
                table: "tokens",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "ix_users_active_role",
                schema: "mosquito",
                table: "users",
                columns: new[] { "is_active", "role_type" });

            migrationBuilder.CreateIndex(
                name: "ix_users_active_verified",
                schema: "mosquito",
                table: "users",
                columns: new[] { "is_active", "is_email_verified" });

            migrationBuilder.CreateIndex(
                name: "ix_users_created_at",
                schema: "mosquito",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_email_unique",
                schema: "mosquito",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email_verified",
                schema: "mosquito",
                table: "users",
                column: "is_email_verified");

            migrationBuilder.CreateIndex(
                name: "ix_users_id_active",
                schema: "mosquito",
                table: "users",
                column: "id",
                filter: "is_deleted = false AND is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                schema: "mosquito",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_users_last_login",
                schema: "mosquito",
                table: "users",
                column: "last_login_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_users_lockout_end",
                schema: "mosquito",
                table: "users",
                column: "lockout_end_date_utc",
                filter: "lockout_end_date_utc IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_mobile_unique",
                schema: "mosquito",
                table: "users",
                column: "mobile",
                unique: true,
                filter: "mobile IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_role_type",
                schema: "mosquito",
                table: "users",
                column: "role_type");

            migrationBuilder.CreateIndex(
                name: "ix_users_search_names",
                schema: "mosquito",
                table: "users",
                columns: new[] { "first_name", "last_name", "username" });

            migrationBuilder.CreateIndex(
                name: "ix_users_username_unique",
                schema: "mosquito",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mosquitoes",
                schema: "mosquito");

            migrationBuilder.DropTable(
                name: "tokens",
                schema: "mosquito");

            migrationBuilder.DropTable(
                name: "users",
                schema: "mosquito");
        }
    }
}
