using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;

namespace Track_My_Work
{
    public class Service : IService
    {
        public StatInfo GetStatInfo()
        {
            try
            {
                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                }

                return StatProvider.GetStatisticsInfo();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new StatInfo();
        }
        public Response GetDayInfo(int day)
        {
            try
            {
                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                }

                var actTime = Tracker.GetActiveTime(DateTime.Now.AddDays(day));
                var totalMinutesOfActiveTime = actTime.Item1?.TotalMinutes ?? 0;
                var workTime = Tracker.GetWorkTime(DateTime.Now.AddDays(day));
                var totalMinutesOfWorkTime = workTime?.TotalMinutes ?? 0;

                return new Response() { activeTime = totalMinutesOfActiveTime, workTime = totalMinutesOfWorkTime, awayTimes = actTime.Item2 };
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Response();
        }

        public IEnumerable<DayInfo> GetDatasource()
        {
            try
            {
                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.Format = WebMessageFormat.Json;
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                }

                return StatProvider.GetAllDayInfos().OrderByDescending(d => d.Day);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<DayInfo>();
        }

        public class Response
        {
            public double activeTime { get; set; }
            public double workTime { get; set; }
            public int awayTimes { get; set; }

        }        
    }
}
