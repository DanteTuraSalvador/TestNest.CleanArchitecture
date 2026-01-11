using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestNest.Admin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSoftDeleteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "SocialMediaPlatforms",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "SocialMediaPlatforms",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SocialMediaPlatforms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "SocialMediaPlatforms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "SocialMediaPlatforms",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstablishmentSocialMedia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EstablishmentSocialMedia",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EstablishmentSocialMedia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EstablishmentSocialMedia",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstablishmentSocialMedia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EstablishmentSocialMedia",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EstablishmentSocialMedia",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Establishments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "Establishments",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Establishments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "Establishments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Establishments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Establishments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "Establishments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstablishmentPhones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EstablishmentPhones",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EstablishmentPhones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EstablishmentPhones",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstablishmentPhones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EstablishmentPhones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EstablishmentPhones",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstablishmentMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EstablishmentMembers",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EstablishmentMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EstablishmentMembers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstablishmentMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EstablishmentMembers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EstablishmentMembers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstablishmentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EstablishmentContacts",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EstablishmentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EstablishmentContacts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstablishmentContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EstablishmentContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EstablishmentContacts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EstablishmentAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EstablishmentAddresses",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EstablishmentAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EstablishmentAddresses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EstablishmentAddresses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EstablishmentAddresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EstablishmentAddresses",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "Employees",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "Employees",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "Employees",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EmployeeRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedOnUtc",
                table: "EmployeeRoles",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EmployeeRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedOnUtc",
                table: "EmployeeRoles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmployeeRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "EmployeeRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedOnUtc",
                table: "EmployeeRoles",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "SocialMediaPlatforms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EstablishmentSocialMedia");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EstablishmentPhones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EstablishmentMembers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EstablishmentContacts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EstablishmentAddresses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "ModifiedOnUtc",
                table: "EmployeeRoles");
        }
    }
}
