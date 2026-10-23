using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeInsurance",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeInsurance",
                table: "Bookings");
        }
    }
}
