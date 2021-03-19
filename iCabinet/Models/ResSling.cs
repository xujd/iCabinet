using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Models
{
    class ResSling
    { 
        public string ResID { get; set; }
        public string RfID { get; set; }
        public string ResName { get; set; }
        public string CreatedTime { get; set; }
        public string TakeStaffName { get; set; }
        public string TakeTime { get; set; }
        public string ReturnPlanTime { get; set; }
        public string CabinetName { get; set; }
        public string CabinetLocation { get; set; }
        public string CabinetGrid { get; set; }

        public string UsedDuration
        {
            get
            {
                return string.IsNullOrEmpty(TakeTime) ? "" : this.formatDuring((DateTime.Now - DateTime.Parse(TakeTime)).TotalMilliseconds);
            }
        }

        private string formatDuring(double mss)
        {
            var days = Convert.ToInt32(mss / (1000 * 60 * 60 * 24));
            var hours = Convert.ToInt32((mss % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Convert.ToInt32((mss % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = (mss % (1000 * 60)) / 1000;
            return days + "天" + hours + "小时" + minutes + "分钟";
        }

        public ResSling Clone()
        {
            ResSling temp = new ResSling();
            temp.ResID = this.ResID;
            temp.RfID = this.RfID;
            temp.ResName = this.ResName;
            temp.CreatedTime = this.CreatedTime;
            temp.TakeStaffName = this.TakeStaffName;
            temp.TakeTime = this.TakeTime;
            temp.ReturnPlanTime = this.ReturnPlanTime;
            temp.CabinetName = this.CabinetName;
            temp.CabinetLocation = this.CabinetLocation;
            temp.CabinetGrid = this.CabinetGrid;

            return temp;
        }
    }
}
