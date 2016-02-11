using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VideoTherapy.Utils
{
    public class MD5Hash
    {
        public static String CalculateMD5Hash(String input)
        {
            // step 1, calculate MD5 hash from input

            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)

                {
                    sb.Append(data[i].ToString("x2"));
                }

                return sb.ToString();
            }          
        }

    }
}
