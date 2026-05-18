using System.Data;
using LibraryManagement.DAL.Contexts;
using LibraryManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace LibraryManagement.DAL.Repositories.Implementations
{
    /// <summary>
    /// Delegates all transactional borrowing logic to PostgreSQL stored procedures.
    /// proc_borrow_book and proc_return_book own their own COMMIT/ROLLBACK —
    /// no C# transaction management needed.
    ///
    /// Uses NpgsqlCommand directly (not ExecuteSqlRaw) because EF Core's SQL parser
    /// rejects OUT-only parameters in CALL statements.
    /// </summary>
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly LibraryContext _context;

        public BorrowingRepository(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>Calls proc_borrow_book and returns its result message.</summary>
        public string BorrowBook(int memberId, int bookCopyId)
        {
            return CallProcedure(
                "CALL proc_borrow_book(@p_member_id, @p_copy_id, @p_result)",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("p_member_id", NpgsqlDbType.Integer, memberId);
                    cmd.Parameters.AddWithValue("p_copy_id",   NpgsqlDbType.Integer, bookCopyId);
                    var outParam = new NpgsqlParameter("p_result", NpgsqlDbType.Text)
                        { Direction = ParameterDirection.InputOutput, Value = DBNull.Value };
                    cmd.Parameters.Add(outParam);
                    return outParam;
                });
        }

        /// <summary>Calls proc_return_book and returns its result message.</summary>
        public string ReturnBook(int borrowingId, bool isDamaged)
        {
            return CallProcedure(
                "CALL proc_return_book(@p_borrowing_id, @p_is_damaged, @p_result)",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("p_borrowing_id", NpgsqlDbType.Integer, borrowingId);
                    cmd.Parameters.AddWithValue("p_is_damaged",   NpgsqlDbType.Boolean, isDamaged);
                    var outParam = new NpgsqlParameter("p_result", NpgsqlDbType.Text)
                        { Direction = ParameterDirection.InputOutput, Value = DBNull.Value };
                    cmd.Parameters.Add(outParam);
                    return outParam;
                });
        }

        // ── Helper ────────────────────────────────────────────────────────────
        // Opens the underlying Npgsql connection, executes the CALL, and reads
        // the OUT parameter value. Uses InputOutput direction — Npgsql requires
        // this for CALL OUT parameters (pure Output is rejected by the SQL parser).
        private string CallProcedure(string sql, Func<NpgsqlCommand, NpgsqlParameter> setup)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
            bool opened = false;

            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    opened = true;
                }

                using var cmd = new NpgsqlCommand(sql, conn);
                var outParam = setup(cmd);
                cmd.ExecuteNonQuery();
                return outParam.Value?.ToString() ?? "Unknown error occurred.";
            }
            finally
            {
                if (opened) conn.Close();
            }
        }
    }
}
