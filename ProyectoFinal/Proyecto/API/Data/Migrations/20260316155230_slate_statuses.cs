using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class slate_statuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)1,
                column: "name",
                value: "PENDING");

            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)2,
                column: "name",
                value: "OPEN");

            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)3,
                column: "name",
                value: "CLOSED");

            migrationBuilder.InsertData(
                table: "scrutiny_statuses",
                columns: new[] { "id", "name", "updated_at" },
                values: new object[] { (short)4, "SIGNED", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)4);

            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)1,
                column: "name",
                value: "OPEN");

            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)2,
                column: "name",
                value: "CLOSED");

            migrationBuilder.UpdateData(
                table: "scrutiny_statuses",
                keyColumn: "id",
                keyValue: (short)3,
                column: "name",
                value: "SIGNED");
        }
    }
}
