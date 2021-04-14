using iCabinet.Core;
using iCabinet.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Services
{
    class Service
    {
        // 待取出：take_time等于空，且return_time等于空
        public static async Task<List<ResSling>> GetBorrowingSlingsByStaff(int staffId)
        {
            List<ResSling> dataList = new List<ResSling>();
            var sqlStr = GetBorrowingSlingSql(staffId);
            var list = await PgUtil.QueryAsync(sqlStr);
            list.ForEach(item =>
            {
                dataList.Add(CreateSling(item));
            });

            return dataList;
        }

        // 待归还：take_time不等于空，且return_time等于空
        public static async Task<List<ResSling>> GetBorrowedSlingsByStaff(int staffId)
        {
            List<ResSling> dataList = new List<ResSling>();
            var sqlStr = GetBorrowedSlingSql(staffId);
            var list = await PgUtil.QueryAsync(sqlStr);
            list.ForEach(item =>
            {
                dataList.Add(CreateSling(item));
            });

            return dataList;
        }

        private static ResSling CreateSling(List<object> list)
        {
            var resSling = new ResSling();
            resSling.ResID = list[0].ToString();
            resSling.RfID = list[1].ToString();
            resSling.ResName = list[2].ToString();
            resSling.CreatedTime = list[7].ToString();
            resSling.TakeStaffName = list[4].ToString();
            resSling.TakeTime = list[5].ToString();
            resSling.ReturnPlanTime = list[6].ToString();
            resSling.CabinetName = list[10].ToString();
            resSling.CabinetLocation = list[11].ToString();
            resSling.CabinetGrid = list[8].ToString();

            return resSling;
        }

        private static string GetBorrowingSlingSql(int staffId)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            return "SELECT t1.res_id, t1.rf_id, t1.res_name, t1.take_staff_id, t1.take_staff_name, " +
                   "t1.take_time, t1.return_plan_time, t1.created_at, " +
                   "t4.grid_no, t4.cabinet_id, t5.name AS cabinet_name, t5.location AS cabinet_location " +
                   "FROM t_res_use_log t1 " +
                   "LEFT JOIN t_res_cabinet_grid t4 ON t1.res_id = t4.in_res_id " +
                   "LEFT JOIN t_res_cabinet t5 on t5.id = t4.cabinet_id " +
                  string.Format("WHERE t1.take_staff_id = {0} AND t1.return_time IS NULL AND t1.take_time is NULL AND t4.cabinet_id = {1} ", staffId, cabinetID);
        }
        private static string GetBorrowedSlingSql(int staffId)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            return "SELECT t1.res_id, t1.rf_id, t1.res_name, t1.take_staff_id, t1.take_staff_name, " +
                   "t1.take_time, t1.return_plan_time, t1.created_at, " +
                   "t4.grid_no, t4.cabinet_id, t5.name AS cabinet_name, t5.location AS cabinet_location " +
                   "FROM t_res_use_log t1 " +
                   "LEFT JOIN t_res_cabinet_grid t4 ON t1.res_id = t4.in_res_id " +
                   "LEFT JOIN t_res_cabinet t5 on t5.id = t4.cabinet_id " +
                  string.Format("WHERE t1.take_staff_id = {0} AND t1.return_time IS NULL AND t1.take_time is NOT NULL AND t4.cabinet_id = {1} ", staffId, cabinetID);
        }

        public static async Task<string[]> GetSlingGrid(string rfId)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            string[] data = null;

            var sqlStr = "SELECT grid_no, cabinet_id FROM t_res_cabinet_grid " +
                         string.Format("WHERE in_res_id = (SELECT id FROM t_res_sling WHERE rf_id = '{0}') AND cabinet_id = {1} AND deleted_at IS NULL ", rfId, cabinetID);
            var list = await PgUtil.QueryAsync(sqlStr);
            if (list.Count > 0)
            {
                data = new string[2];
                data[0] = list[0][0].ToString();
                data[1] = list[0][1].ToString();
            }

            return data;
        }

        public static async Task<bool> ReturnSling(int staffId, string staffName, string rfId)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            List<string> sqlList = new List<string>();

            var sqlFormat = "UPDATE t_res_use_log SET return_staff_name = '{0}', " +
                "return_staff_id = {1}, " +
                "return_time = '{2}' WHERE  return_time IS NULL AND rf_id = '{3}'";
            var sqlStr = string.Format(sqlFormat, staffName, staffId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), rfId);
            // var data = await PgUtil.ExecuteAsync(sqlStr);
            sqlList.Add(sqlStr);

            sqlStr = "UPDATE t_res_cabinet_grid SET is_out = 0 " +
                string.Format("WHERE in_res_id = (SELECT id FROM t_res_sling WHERE rf_id = '{0}') AND cabinet_id = {1}", rfId, cabinetID);
            // data = await PgUtil.ExecuteAsync(sqlStr);
            sqlList.Add(sqlStr);

            sqlStr = "UPDATE t_res_sling SET use_status = 1 " +
                string.Format("WHERE rf_id = '{0}'", rfId);
            sqlList.Add(sqlStr);

            var data = await PgUtil.ExecTransactionAsync(sqlList);


            return data;
        }

        public static async Task<bool> TakeSling(int staffId, string rfId)
        {
            var sqlFormat = "UPDATE t_res_use_log SET take_time = '{0}' " +
                "WHERE take_time IS NULL AND take_staff_id = {1} AND rf_id = '{2}'";
            var sqlStr = string.Format(sqlFormat, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), staffId, rfId);
            var data = await PgUtil.ExecuteAsync(sqlStr);

            return data;
        }

        public static async Task<string> GetStaffName(int staffId)
        {
            var sqlStr = string.Format("SELECT name FROM t_sys_staff WHERE id = {0} AND deleted_at IS NULL", staffId);
            var list = await PgUtil.QueryAsync(sqlStr);
            if (list.Count > 0)
            {
                return list[0][0].ToString();
            }

            return "";
        }

        // -1-查询错误，0-没有空余，其他正常格子
        public static async Task<int> GetAvailableGrid(int cabinetId)
        {
            var sqlStr = string.Format("SELECT t1.grid_count, t2.grid_no FROM t_res_cabinet t1 " +
                    "JOIN  t_res_cabinet_grid t2 ON t2.cabinet_id = t1.id " +
                    "WHERE t1.id = {0} AND t2.deleted_at IS NULL " +
                    "ORDER BY t2.grid_no ASC", cabinetId);
            var list = await PgUtil.QueryAsync(sqlStr);
            if (list.Count > 0)
            {
                int count = int.Parse(list[0][0].ToString());
                if (list.Count == count) // 已存满
                {
                    return 0;
                }
                for (var i = 1; i <= count; i++)
                {
                    var isUsed = false;
                    for (var j = 0; j < list.Count; j++)
                    {
                        if (i.ToString() == list[j][1].ToString())
                        {
                            isUsed = true;
                            break;
                        }
                    }
                    if (!isUsed)
                    {
                        return i; // 找到空闲位置
                    }
                }
            }

            return -1; // 错误
        }


        public static async Task<List<object>> GetSlingIDByRFID(string rfId)
        {
            var sqlStr = string.Format("SELECT id FROM t_res_sling WHERE rf_id = '{0}' AND deleted_at IS NULL", rfId);
            var list = await PgUtil.QueryAsync(sqlStr);

            return list.Count > 0 ? list[0] : null;
        }


        public static async Task<List<object>> GetNewSlingByRFID(string rfId)
        {
            var sqlStr = string.Format("SELECT grid_no FROM t_res_cabinet_grid WHERE in_res_id = (SELECT id FROM t_res_sling WHERE rf_id = '{0}' AND deleted_at IS NULL)  AND deleted_at IS NULL", rfId);
            var list = await PgUtil.QueryAsync(sqlStr);

            return list.Count > 0 ? list[0] : null;
        }

        public static async Task<int> PutNewSling(int staffId, string staffName, string rfId, int cabinetId, int gridNo)
        {
            // 查
            var newData = await GetSlingIDByRFID(rfId);
            if (newData == null)
            {
                return -2; // 未找到
            }

            // 位置
            var curTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sqlFormat = "INSERT INTO t_res_cabinet_grid (created_at, updated_at, grid_no, cabinet_id, in_res_id, is_out) VALUES ('{0}', '{1}', {2}, {3}, {4}, 0)";
            var sqlStr = string.Format(sqlFormat, curTime, curTime, gridNo, cabinetId, newData[0]);
            var data = await PgUtil.ExecuteAsync(sqlStr);
            
            return data ? 0 : -1;
        }


        // 1-rfID已存在，0-成功，1-失败
        public static async Task<int> PutSling(int staffId, string staffName, string rfId, int cabinetId, int gridNo)
        {
            var data0 = await GetSlingIDByRFID(rfId);
            if (data0 != null)
            {
                return 1; // 已存在
            }

            // 写
            var sqlFormat = "INSERT INTO t_res_sling (created_at, updated_at, rf_id, name, use_status, inspect_status, put_time) VALUES ('{0}', '{1}', '{2}', '{3}', 1, 1, '{4}')";
            var curTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sqlStr = string.Format(sqlFormat, curTime, curTime, rfId, rfId, curTime);
            var data = await PgUtil.ExecuteAsync(sqlStr);

            if(!data) // 添加失败
            {
                return -1;
            }
            // 查
            var newData = await GetSlingIDByRFID(rfId);
            if (newData == null)
            {
                return -1; // 写失败
            }

            // 位置
            sqlFormat = "INSERT INTO t_res_cabinet_grid (created_at, updated_at, grid_no, cabinet_id, in_res_id, is_out) VALUES ('{0}', '{1}', {2}, {3}, {4}, 0)";
            sqlStr = string.Format(sqlFormat, curTime, curTime, gridNo, cabinetId, newData[0]);
            data = await PgUtil.ExecuteAsync(sqlStr);

            if(!data) { // 如果出错删除第一步
                sqlFormat = "DELETE FROM  t_res_sling WHERE id = {0}";
                sqlStr = string.Format(sqlFormat, newData[0]);
                data = await PgUtil.ExecuteAsync(sqlStr);
                return -1;
            }

            return data ? 0 : -1;
        }
    }
}
