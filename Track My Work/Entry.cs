using System;
using Microsoft.Win32;

namespace Track_My_Work
{
    public class Entry
    {
        public DateTime Time { get; set; }
        public SessionSwitchReason Type { get; set; }
        public string User { get; set; }
    }

    public class DayInfo
    {
        public DateTime Day { get; set; }
        public double? ActiveTime { get; set; }
        public double? WorkTime { get; set; }
        public int AwayTimes { get; set; }

    }
}
