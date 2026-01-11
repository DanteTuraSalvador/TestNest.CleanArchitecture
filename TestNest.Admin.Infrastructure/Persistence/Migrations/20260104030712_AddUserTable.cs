using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestNest.Admin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "NVARCHAR(100)", maxLength: 100, nullable: false),
                    RefreshToken = table.Column<string>(type: "NVARCHAR(256)", maxLength: 256, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshToken",
                table: "Users",
                column: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
