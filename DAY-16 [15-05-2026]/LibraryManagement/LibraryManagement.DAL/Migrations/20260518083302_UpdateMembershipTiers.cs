using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMembershipTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Basic");

            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaxBorrowDays", "MaxBorrowings", "Name" },
                values: new object[] { 10, 3, "Student" });

            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaxBorrowDays", "MaxBorrowings" },
                values: new object[] { 15, 5 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Student");

            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaxBorrowDays", "MaxBorrowings", "Name" },
                values: new object[] { 14, 5, "Regular" });

            migrationBuilder.UpdateData(
                table: "MembershipTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaxBorrowDays", "MaxBorrowings" },
                values: new object[] { 30, 10 });
        }
    }
}
