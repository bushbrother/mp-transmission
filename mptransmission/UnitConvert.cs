using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mptransmission
{
    public class UnitConvert
    {
        static public string SizeToString(long p)
        {
            float fp = p;
            if (fp < 1024)
            {
                return String.Format("{0:F2} B", fp);
            };

            fp /= 1024;
            if (fp < 1024)
            {
                return String.Format("{0:F2} KB", fp);
            };

            fp /= 1024;
            if (fp < 1024)
            {
                return String.Format("{0:F2} MB", fp);
            };

            fp /= 1024;
            if (fp < 1024)
            {
                return String.Format("{0:F2} GB", fp);
            };

            fp /= 1024;
            if (fp < 1024)
            {
                return String.Format("{0:F2} TB", fp);
            };
            return "";
        }

        static public string TimeRemainingToString(long seconds)
        {
            if (seconds < 60)
            {
                return string.Format("{0}s", seconds);
            }

            long minutes = seconds / 60;
            seconds -= minutes * 60;

            if (minutes < 60)
            {
                return string.Format("{0}min {1}s", minutes, seconds);
            }

            long hours = minutes / 60;
            minutes -= hours * 60;

            if (hours < 24)
            {
                return string.Format("{0}h {1}min {2}s", hours, minutes, seconds);
            }

            long days = hours / 24;
            hours -= days * 24;

            if (days < 7)
            {
                return string.Format("{0}d {1}h {2}min", days, hours, minutes);
            }

            long weeks = days / 7;
            days -= weeks * 7;
            return string.Format("{0}w {1}d {2}h", weeks, days, hours);
        }
        static public string TransferSpeedToString(long bytes)
        {
            return SizeToString(bytes) + "/s";
        }
    }
}
