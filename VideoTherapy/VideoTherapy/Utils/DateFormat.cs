using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Utils
{
    public static class DateFormat
    {
        public static string FormatDate(DateTime date)
        {
            StringBuilder d = new StringBuilder();
            d.Append(date.Day);
            d.Append(".");
            d.Append(date.Month);
            d.Append(".");
            d.Append(date.ToString("yy"));

            return d.ToString();
        }
    }
}
