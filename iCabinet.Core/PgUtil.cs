﻿using Npgsql;
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
        public static string CONN_STR = "Host=localhost;Username=postgres;Password=123456;Database=cmkit";

        static PgUtil ()
        {
            CONN_STR = AESUtil.AESDecrypt(ConfigurationManager.ConnectionStrings["npg"].ToString());
        }

        private static string GetConnection(string connection = null)
        {
            var connString = CONN_STR;
            if (connection != null)
            {
                connString = connection;
            }

            return connString;
        }

        public static List<List<object>> Query(string sql, string connection = null)
        {
            var connString = GetConnection(connection);

            List<List<object>> result = new List<List<object>>();
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
                            var rowData = new object[reader.FieldCount];
                            reader.GetValues(rowData);

                            result.Add(rowData.ToList());
                            Console.WriteLine(string.Join(",", rowData));
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

        public static async Task<List<List<object>>> QueryAsync(string sql, string connection = null)
        {
            var connString = GetConnection(connection);

            List<List<object>> result = new List<List<object>>();
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
                            var rowData = new object[reader.FieldCount];
                            reader.GetValues(rowData);

                            result.Add(rowData.ToList());
                            Console.WriteLine(string.Join(",", rowData));
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

        public static bool Execute(string sql, string connection = null)
        {
            var connString = GetConnection(connection);

            bool result = false;
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

        public static async Task<bool> ExecuteAsync(string sql, string connection = null)
        {
            var connString = GetConnection(connection);

            bool result = false;
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
