using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;

namespace Track_My_Work
{
    class Tracker
    {
        private static readonly List<SessionSwitchReason> startSessionReasons = new List<SessionSwitchReason>() { SessionSwitchReason.ConsoleConnect, SessionSwitchReason.RemoteConnect, SessionSwitchReason.SessionLogon, SessionSwitchReason.SessionUnlock };
        private static readonly List<SessionSwitchReason> stopSessionReasons = new List<SessionSwitchReason>() { SessionSwitchReason.ConsoleDisconnect, SessionSwitchReason.RemoteDisconnect, SessionSwitchReason.SessionLogoff, SessionSwitchReason.SessionLock };

        public static void SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            OnSessionSwitch(e.Reason);
        }

        public static TimeSpan? GetWorkTime(DateTime day)
        {
            var dayData = DBProvider.ReadDay(day);

            var startTime = dayData.FirstOrDefault(e => startSessionReasons.Contains(e.Type));
            var endTime = dayData.LastOrDefault(e => stopSessionReasons.Contains(e.Type));

            if (startTime == null || endTime == null || endTime.Time < startTime.Time)
                return null;
            
            return endTime.Time - startTime.Time;
        }

        public static Tuple<TimeSpan?, int> GetActiveTime(DateTime day)
        {
            var dayData = DBProvider.ReadDay(day);
            var activeTime = new TimeSpan();
            var awayTimes = 0;
            while (dayData.Any())
            {
                var startTime = dayData.FirstOrDefault(e => startSessionReasons.Contains(e.Type));
                var endTime = dayData.FirstOrDefault(e => stopSessionReasons.Contains(e.Type) && e.Time > startTime.Time);

                if (startTime == null || endTime == null || endTime.Time < startTime.Time)
                    break;

                activeTime += endTime.Time - startTime.Time;

                dayData.RemoveAll(e => e.Time <= endTime.Time);
                awayTimes++;
            }
            if (awayTimes>0)
            {
                awayTimes--;
            }
            return new Tuple<TimeSpan?, int>(activeTime, awayTimes);
        }

        private static void OnSessionSwitch(SessionSwitchReason reason)
        {
            try
            {
                var time = DateTime.Now;
                var user = WindowsIdentity.GetCurrent().Name;

                DBProvider.Insert(time, reason, user);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            
        }


    }
}
