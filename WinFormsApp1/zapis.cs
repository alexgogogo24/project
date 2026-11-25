using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WinFormsApp1
{
    public static class DbHelper
    {
        private static readonly string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=mini games;User Id=game_app;Password=123;TrustServerCertificate=True;";

        public static int Execute(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetData(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }
        public static bool SaveGameRecord(string tableName, string columnName, int playerId, int newRecord)
        {
            string query = $@"
                IF EXISTS (SELECT 1 FROM [{tableName}] WHERE Id_p = @playerId)
                BEGIN
                    UPDATE [{tableName}] 
                    SET [{columnName}] = @newRecord 
                    WHERE Id_p = @playerId AND [{columnName}] < @newRecord
                END
                ELSE
                BEGIN
                    INSERT INTO [{tableName}] (Id_p, [{columnName}]) 
                    VALUES (@playerId, @newRecord)
                END";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@playerId", playerId),
                new SqlParameter("@newRecord", newRecord)
            };

            int rowsAffected = Execute(query, parameters);
            return rowsAffected > 0;
        }
    }
}