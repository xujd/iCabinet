using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class PgUtil
    {
        public static String CONN_STR = "Host=localhost;Username=postgres;Password=123456;Database=vdcdb";

        static PgUtil ()
        {
            CONN_STR = ConfigurationManager.ConnectionStrings["npg"].ToString();
        }

        private static String GetConnection(String connection = null)
        {
            var connString = CONN_STR;
            if (connection != null)
            {
                connString = connection;
            }

            return connString;
        }

        public static List<List<Object>> Query(String sql, String connection = null)
        {
            var connString = GetConnection(connection);

            List<List<Object>> result = new List<List<Object>>();
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var rowData = new Object[reader.FieldCount];
                            reader.GetValues(rowData);

                            result.Add(rowData.ToList());
                            Console.WriteLine(String.Join(",", rowData));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("ERROR-DB：" + ex.Message);
            }

            return result;
        }

        public static async Task<List<List<Object>>> QueryAsync(String sql, String connection = null)
        {
            var connString = GetConnection(connection);

            List<List<Object>> result = new List<List<Object>>();
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var rowData = new Object[reader.FieldCount];
                            reader.GetValues(rowData);

                            result.Add(rowData.ToList());
                            Console.WriteLine(String.Join(",", rowData));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("ERROR-DB：" + ex.Message);
            }

            return result;
        }

        public static Boolean Execute(String sql, String connection = null)
        {
            var connString = GetConnection(connection);

            Boolean result = false;
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        result = cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("ERROR-DB：" + ex.Message);
            }

            return result;
        }

        public static async Task<Boolean> ExecuteAsync(String sql, String connection = null)
        {
            var connString = GetConnection(connection);

            Boolean result = false;
            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        result = await cmd.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog("ERROR-DB：" + ex.Message);
            }

            return result;
        }
    }
}
