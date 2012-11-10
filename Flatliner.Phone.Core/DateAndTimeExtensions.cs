namespace Flatliner.Phone.Core
{
    using System;
    using System.Globalization;
    using System.Text;
    
    public static class DateAndTimeExtensions
    {
        public static DateTime ParseToLocalDateTimeEnsurePast(this string dateTimeText)
        {
            if (string.IsNullOrWhiteSpace(dateTimeText))
            {
                return DateTime.Now.AddSeconds(-1);
            }

            DateTime result;
            DateTimeOffset createdDate;
            if (DateTimeOffset.TryParse(dateTimeText, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
            {
                result = DateTime.SpecifyKind(createdDate.ToLocalTime().DateTime, DateTimeKind.Local);
            }
            else
            {
                result = DateTime.Now.AddSeconds(-1);
            }

            if (result > DateTime.Now)
            {
                result = DateTime.Now.AddSeconds(-1);
            }

            return result;
        }

        /// <summary>
        /// Converts the TimeSpan to a string that represents the time in the shortest way possible (to a minute minimum e.g. 0:01 is 1 second).
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string ToFormattedString(this TimeSpan timeSpan)
        {
            var sb = new StringBuilder();
            if (timeSpan.TotalHours >= 1)
            {
                sb.AppendFormat("{0:#0}", (int)timeSpan.TotalHours);
                sb.Append(":");
                sb.AppendFormat("{0:00}", timeSpan.Minutes);
                sb.Append(":");
                sb.AppendFormat("{0:00}", timeSpan.Seconds);
            } 
            else
            {
                sb.AppendFormat("{0:#0}", timeSpan.Minutes);
                sb.Append(":");
                sb.AppendFormat("{0:00}", timeSpan.Seconds);
            }

            return sb.ToString();
        }
    }
}
