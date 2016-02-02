using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static object GetStatisticsInfo()
        {
            long averageStartTime = (long) DBProvider.StartTimes().Average(d=>d.Ticks);
            long averageEndTime = (long)DBProvider.EndTimes().Average(d => d.Ticks);
            
            return new StatInfo() {averageStartTime = new TimeSpan(averageStartTime), averageEndTime = new TimeSpan(averageEndTime) };
        }
    }

    class StatInfo
    {
        public TimeSpan averageStartTime { get; set; }
        public TimeSpan averageEndTime { get; set; }
    }
}
