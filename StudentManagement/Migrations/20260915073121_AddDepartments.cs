using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            // Data migration: seed a fallback department, seed one department per
            // distinct existing Courses.Department string, backfill both FK columns
            // from that data, then drop the old string column. This has to happen
            // here (not as a separate migration) because it needs the Department
            // string column to still exist to read the values out of.
            migrationBuilder.Sql(
                "INSERT INTO [Departments] ([Name]) VALUES (N'Unassigned');");

            migrationBuilder.Sql(@"
                INSERT INTO [Departments] ([Name])
                SELECT DISTINCT LTRIM(RTRIM([Department]))
                FROM [Courses]
                WHERE [Department] IS NOT NULL
                  AND LTRIM(RTRIM([Department])) <> ''
                  AND LTRIM(RTRIM([Department])) <> N'Unassigned';");

            migrationBuilder.Sql(@"
                UPDATE c
                SET c.[DepartmentId] = d.[DepartmentId]
                FROM [Courses] c
                INNER JOIN [Departments] d ON d.[Name] = LTRIM(RTRIM(c.[Department]))
                WHERE c.[Department] IS NOT NULL AND LTRIM(RTRIM(c.[Department])) <> '';");

            migrationBuilder.Sql(@"
                UPDATE [Courses]
                SET [DepartmentId] = (SELECT [DepartmentId] FROM [Departments] WHERE [Name] = N'Unassigned')
                WHERE [DepartmentId] IS NULL;");

            migrationBuilder.Sql(@"
                UPDATE [Students]
                SET [DepartmentId] = (SELECT [DepartmentId] FROM [Departments] WHERE [Name] = N'Unassigned')
                WHERE [DepartmentId] IS NULL;");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Students_DepartmentId",
                table: "Students",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DepartmentId",
                table: "Courses",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Students_DepartmentId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DepartmentId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Courses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
