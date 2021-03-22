using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCabinet.Core
{
    public class BCC
    {
        public static string CheckXOR(string src)
        {
            byte checkCode = 0;
            string[] ss = src.Split(' ');
            byte[] message = new byte[ss.Length];
            for (var i = 0; i < ss.Length; i++)
            {
                checkCode ^= Convert.ToByte(Convert.ToInt32(ss[i], 16));
            }

            return checkCode.ToString("X2");
        }
    }
}
