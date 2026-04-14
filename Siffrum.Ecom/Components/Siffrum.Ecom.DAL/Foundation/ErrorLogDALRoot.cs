using Siffrum.Ecom.DomainModels.Foundation;
using System.Data;
using Npgsql;

namespace Siffrum.Ecom.DAL.Foundation
{
    public class ErrorLogDALRoot
    {
        private readonly string _connectionStr;

        public ErrorLogDALRoot(string connectionStr)
        {
            _connectionStr = connectionStr;
        }

        public virtual async Task<bool> SaveErrorObjectInDb(ErrorLogRoot errorLog)
        {
            await using var conn = new NpgsqlConnection(_connectionStr);

            const string commandText = @"
        INSERT INTO ""ErrorLogRoots""
        (
            ""LoginUserId"",
            ""UserRoleType"",
            ""CreatedByApp"",
            ""CreatedOnUTC"",
            ""LogMessage"",
            ""LogStackTrace"",
            ""LogExceptionData"",
            ""InnerException"",
            ""TracingId"",
            ""Caller"",
            ""RequestObject"",
            ""ResponseObject"",
            ""AdditionalInfo""
        )
        VALUES
        (
            @LoginUserId,
            @UserRoleType,
            @CreatedByApp,
            @CreatedOnUTC,
            @LogMessage,
            @LogStackTrace,
            @LogExceptionData,
            @InnerException,
            @TracingId,
            @Caller,
            @RequestObject,
            @ResponseObject,
            @AdditionalInfo
        );";

            await using var insertLogCommand = new NpgsqlCommand(commandText, conn);

            var createdOn = errorLog.CreatedOnUTC == default
                ? DateTime.UtcNow
                : errorLog.CreatedOnUTC;

            createdOn = DateTime.SpecifyKind(createdOn, DateTimeKind.Unspecified);

            insertLogCommand.Parameters.AddWithValue("@LoginUserId",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.LoginUserId ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@UserRoleType",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.UserRoleType ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@CreatedByApp",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.CreatedByApp ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@CreatedOnUTC",
                NpgsqlTypes.NpgsqlDbType.Timestamp,createdOn);
            insertLogCommand.Parameters.AddWithValue("@LogMessage",
                NpgsqlTypes.NpgsqlDbType.Text, errorLog.LogMessage);

            insertLogCommand.Parameters.AddWithValue("@LogStackTrace",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.LogStackTrace ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@LogExceptionData",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.LogExceptionData ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@InnerException",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.InnerException ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@TracingId",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.TracingId ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@Caller",
                NpgsqlTypes.NpgsqlDbType.Text, errorLog.Caller);

            insertLogCommand.Parameters.AddWithValue("@RequestObject",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.RequestObject ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@ResponseObject",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.ResponseObject ?? DBNull.Value);

            insertLogCommand.Parameters.AddWithValue("@AdditionalInfo",
                NpgsqlTypes.NpgsqlDbType.Text, (object?)errorLog.AdditionalInfo ?? DBNull.Value);

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            return await insertLogCommand.ExecuteNonQueryAsync() > 0;
        }        
    }
}
