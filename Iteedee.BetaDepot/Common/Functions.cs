using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}