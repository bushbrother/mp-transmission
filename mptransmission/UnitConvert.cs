// mp-transmission
// http://code.google.com/p/mp-transmission/
// Copyright (C) 2012 Laurie R
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mptransmission
{
    public class UnitConvert
    {
        // Converts a long number to the correct data size string format (KB/MB/GB etc.)
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

        // Converts a long number to a string representing time remaining in Days/Hours/Minutes etc.
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

        // Converts a long number to a string representing transfer spped in KB/s etc.
        static public string TransferSpeedToString(long bytes)
        {
            return SizeToString(bytes) + "/s";
        }
    }
}
