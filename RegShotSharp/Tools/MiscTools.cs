using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegShotSharp.Tools
{
    public static class MiscTools
    {
        public static String GetCurrentTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMdd_HH-mm-ss-ffff");
        }
    }
}
