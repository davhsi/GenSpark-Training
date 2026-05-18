using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculateFineFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id integer)
                RETURNS decimal AS $$
                DECLARE
                    total_fine decimal;
                BEGIN
                    SELECT COALESCE(SUM(""Amount"" - ""AmountPaid""), 0)
                    INTO total_fine
                    FROM ""Fines""
                    WHERE ""MemberId"" = p_member_id AND ""IsPaid"" = false;
                    
                    RETURN total_fine;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS calculate_member_fine(integer);");
        }
    }
}
