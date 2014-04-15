using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Iteedee.BetaDepot.Common
{
    public class Functions
    {
        //Source: http://www.java2s.com/Code/CSharp/Date-Time/Getstheprettydate.htm
        public static string GetPrettyDate(DateTime date, string format)
        {
            // 1. Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.Subtract(date);

            // 2. Get total number of days elapsed.
            Int32 dayDiff = (Int32)s.TotalDays;

            // 3. Get total number of seconds elapsed.
            Int32 secDiff = (Int32)s.TotalSeconds;

            // 4. Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31)
            {
                return date.ToString(format);
            }

            // 5. Handle same-day times.
            if (dayDiff == 0)
            {
                // A. Less than one minute ago.
                if (secDiff < 60)
                {
                    return "just now";
                }

                // B. Less than 2 minutes ago.
                if (secDiff < 120)
                {
                    return "1 minute ago";
                }

                // C.Less than one hour ago.
                if (secDiff < 3600)
                {
                    return String.Format("{0} minutes ago", Math.Floor((double)secDiff / 60));
                }

                // D. Less than 2 hours ago.
                if (secDiff < 7200)
                {
                    return "1 hour ago";
                }

                // E. Less than one day ago.
                if (secDiff < 86400)
                {
                    return String.Format("{0} hours ago", Math.Floor((double)secDiff / 3600));
                }
            }

            // 6. Handle previous days.
            if (dayDiff == 1)
            {
                return "yesterday";
            }

            if (dayDiff < 7)
            {
                return String.Format("{0} days ago", dayDiff);
            }

            if (dayDiff < 31)
            {
                return String.Format("{0} weeks ago", Math.Ceiling((double)dayDiff / 7));
            }

            return date.ToString(format);
        }

        private static MD5 md5 = System.Security.Cryptography.MD5.Create();
        public static string GenerateMD5Hash(string itemToHash)
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(itemToHash);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string FormatFullName(string first, string last)
        {
            return String.Format("{0} {1}", first, last);
        }


        public static string GetBaseUrl()
        {
            string retval = string.Empty;
            if(true)
            {
                var request = HttpContext.Current.Request;
                var appUrl = HttpRuntime.AppDomainAppVirtualPath;

                if (!string.IsNullOrWhiteSpace(appUrl)) appUrl += "/";

                retval = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            }
            //else
            //{
            //    retval = System.Configuration.ConfigurationManager.AppSettings["FullyQualifiedBaseUrl"];
            //    if (string.IsNullOrEmpty(retval))
            //        retval = "http://localhost/";
            //    else
            //    {
            //        if (!retval.EndsWith("/"))
            //            retval = string.Format("{0}/", retval);
            //    }
            //}
            return retval;

            //string fqdnUrl = System.Configuration.ConfigurationManager.AppSettings["FullyQualifiedBaseUrl"];
            //if (string.IsNullOrEmpty(fqdnUrl))
            //    fqdnUrl = "http://localhost/";
            //else
            //{
            //    if (!fqdnUrl.EndsWith("/"))
            //        fqdnUrl = string.Format("{0}/", fqdnUrl);
            //}

            //return fqdnUrl;
        }
    }
}