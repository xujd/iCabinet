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
        // 待取出：take_time等于空，且return_time不等于空
        public static async Task<List<ResSling>> GetBorrowingSlingsByStaff(string staffName)
        {
            List<ResSling> dataList = new List<ResSling>();
            var sqlStr = GetSlingSql(staffName);
            var list = await PgUtil.QueryAsync(sqlStr);
            list.ForEach(item =>
            {
                dataList.Add(CreateSling(item));
            });

            return dataList;
        }

        // 待归还：take_time不等于空，且return_time不等于空
        public static async Task<List<ResSling>> GetBorrowedSlingsByStaff(string staffName)
        {
            List<ResSling> dataList = new List<ResSling>();
            var sqlStr = GetSlingSql(staffName);
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

        private static string GetSlingSql(string staffName)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            return "SELECT t1.res_id, t2.rf_id, t2.name AS res_name, t1.take_staff_id, t3.name AS take_staff_name, " +
                   "t1.take_time, t1.return_plan_time, t1.created_at, " +
                   "t4.grid_no, t4.cabinet_id, t5.name AS cabinet_name, t5.location AS cabinet_location " +
                   "FROM t_res_use_log t1 " +
                   "LEFT JOIN t_res_sling t2 ON t1.res_id = t2.id " +
                   "LEFT JOIN t_sys_staff t3 ON t1.take_staff_id = t3.id " +
                   "LEFT JOIN t_res_cabinet_grid t4 ON t1.res_id = t4.in_res_id " +
                   "LEFT JOIN t_res_cabinet t5 on t5.id = t4.cabinet_id " +
                  string.Format("WHERE t1.take_staff_id = (SELECT id FROM t_sys_staff WHERE name = '{0}') AND t1.return_time IS NULL AND t4.cabinet_id = {1} ", staffName, cabinetID);
        }

        public static async Task<string[]> GetSlingGrid(string rfId)
        {
            var cabinetID = ConfigurationManager.AppSettings["CabinetID"];
            string[] data = null;

            var sqlStr = "SELECT grid_no, cabinet_id FROM t_res_cabinet_grid "+
                         string.Format("WHERE in_res_id = (SELECT id FROM t_res_sling WHERE rf_id = {0}) AND cabinet_id = {1}", rfId, cabinetID);
            var list = await PgUtil.QueryAsync(sqlStr);
            if (list.Count > 0)
            {
                data = new string[2];
                data[0] = list[0][0].ToString();
                data[1] = list[0][1].ToString();
            }

            return data;
        }
    }
}
