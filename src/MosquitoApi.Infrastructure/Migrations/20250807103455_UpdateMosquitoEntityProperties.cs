using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MosquitoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMosquitoEntityProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "modified_by",
                schema: "mosquito",
                table: "mosquitoes");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                schema: "mosquito",
                table: "mosquitoes",
                newName: "updated_by");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                schema: "mosquito",
                table: "mosquitoes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_by",
                schema: "mosquito",
                table: "mosquitoes",
                newName: "UpdatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                schema: "mosquito",
                table: "mosquitoes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                schema: "mosquito",
                table: "mosquitoes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
