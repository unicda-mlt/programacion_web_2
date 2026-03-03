using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class complete_structure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "candidacy_types",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    position = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidacy_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scrutiny_statuses",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scrutiny_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scrutinies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status_id = table.Column<short>(type: "smallint", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scrutinies", x => x.id);
                    table.ForeignKey(
                        name: "scrutinies_fk_statusid",
                        column: x => x.status_id,
                        principalTable: "scrutiny_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scrutiny_signs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scrutiny_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scrutiny_signs", x => x.id);
                    table.ForeignKey(
                        name: "scrutiny_signs_fk_scrutinyid",
                        column: x => x.scrutiny_id,
                        principalTable: "scrutinies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "slates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scrutiny_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    position = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_slates", x => x.id);
                    table.ForeignKey(
                        name: "slates_fk_scrutinyid",
                        column: x => x.scrutiny_id,
                        principalTable: "scrutinies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "slate_candidacies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    slate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    candidacy_type_id = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_slate_candidacies", x => x.id);
                    table.ForeignKey(
                        name: "slate_candidacies_fk_candidacytypeid",
                        column: x => x.candidacy_type_id,
                        principalTable: "candidacy_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "slate_candidacies_fk_slateid",
                        column: x => x.slate_id,
                        principalTable: "slates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scrutiny_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    slate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    student_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issue_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_votes", x => x.id);
                    table.ForeignKey(
                        name: "votes_fk_scrutinyid",
                        column: x => x.scrutiny_id,
                        principalTable: "scrutinies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "votes_fk_slateid",
                        column: x => x.slate_id,
                        principalTable: "slates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "votes_fk_studentid",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "votes_fk_userid",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "scrutiny_statuses",
                columns: new[] { "id", "name", "updated_at" },
                values: new object[,]
                {
                    { (short)1, "OPEN", null },
                    { (short)2, "CLOSED", null },
                    { (short)3, "SIGNED", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_scrutinies_status_id",
                table: "scrutinies",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_scrutiny_signs_scrutiny_id",
                table: "scrutiny_signs",
                column: "scrutiny_id");

            migrationBuilder.CreateIndex(
                name: "ix_slate_candidacies_candidacy_type_id",
                table: "slate_candidacies",
                column: "candidacy_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_slate_candidacies_slate_id",
                table: "slate_candidacies",
                column: "slate_id");

            migrationBuilder.CreateIndex(
                name: "ix_slates_scrutiny_id",
                table: "slates",
                column: "scrutiny_id");

            migrationBuilder.CreateIndex(
                name: "ix_votes_scrutiny_id",
                table: "votes",
                column: "scrutiny_id");

            migrationBuilder.CreateIndex(
                name: "ix_votes_slate_id",
                table: "votes",
                column: "slate_id");

            migrationBuilder.CreateIndex(
                name: "ix_votes_student_id",
                table: "votes",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_votes_user_id",
                table: "votes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scrutiny_signs");

            migrationBuilder.DropTable(
                name: "slate_candidacies");

            migrationBuilder.DropTable(
                name: "votes");

            migrationBuilder.DropTable(
                name: "candidacy_types");

            migrationBuilder.DropTable(
                name: "slates");

            migrationBuilder.DropTable(
                name: "scrutinies");

            migrationBuilder.DropTable(
                name: "scrutiny_statuses");
        }
    }
}
