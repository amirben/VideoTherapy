using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Utils
{
    public static class HttpsReplaceToHttp
    {
        //using as a workaround for replacing https to http
        public static String ReplaceHttpsToHttp(String url)
        {
            return url.Replace("https://", "http://");
        }
    }
}
