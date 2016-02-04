using System;
using System.Collections.Generic;
using System.Linq;

namespace Track_My_Work
{
    static class StatProvider
    {
        public static List<DayInfo> GetAllDayInfos()
        {
            var startDate = DBProvider.GetLogStartDay();
            var endDate = DateTime.Now;

            if (!startDate.HasValue || startDate.Value.Date == endDate.Date)
                return new List<DayInfo>();
                     
            return CalculateDayInfos(startDate.Value, endDate).ToList();
        }
        private static IEnumerable<DayInfo> CalculateDayInfos(DateTime startDate, DateTime endDate) {

            var day = startDate.Date;

            while (day < endDate)
            {
                var activeTime = Tracker.GetActiveTime(day);
                if (activeTime.Item1.HasValue)
                {
                    var ActiveTime = activeTime.Item1.Value.TotalSeconds;
                    var workTime = Tracker.GetWorkTime(day);
                    if (workTime.HasValue)
                    {
                        var WorkTime = workTime.Value.TotalSeconds;

                        yield return new DayInfo() { ActiveTime = ActiveTime, WorkTime = WorkTime, Day = day, AwayTimes = activeTime.Item2};
                    }
                }
                day = day.AddDays(1);
            }
        }

        public static StatInfo GetStatisticsInfo()
        {
            var averageStartTime = TimeSpan.FromTicks((long) DBProvider.StartTimes().Average(d=>d.Ticks)).TotalSeconds;
            var averageEndTime = TimeSpan.FromTicks((long) DBProvider.EndTimes().Average(d=>d.Ticks)).TotalSeconds;
            
            return new StatInfo() {averageStartTime = averageStartTime, averageEndTime = averageEndTime };
        }
    }

    public class StatInfo
    {
        public double averageStartTime { get; set; }
        public double averageEndTime { get; set; }
    }
}
