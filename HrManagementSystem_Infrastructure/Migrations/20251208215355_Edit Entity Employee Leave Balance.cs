using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrMangmentSystem_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditEntityEmployeeLeaveBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLeaveBalances_Tenants_TenantId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EmployeeLeaveBalances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByEmployeeId",
                table: "EmployeeLeaveBalances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmployeeLeaveBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalances_Tenants_TenantId",
                table: "EmployeeLeaveBalances",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLeaveBalances_Tenants_TenantId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropColumn(
                name: "DeletedByEmployeeId",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmployeeLeaveBalances");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalances_Tenants_TenantId",
                table: "EmployeeLeaveBalances",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
