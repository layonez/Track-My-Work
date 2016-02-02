using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Track_My_Work
{
    static class Extensions
    {
        public static IEnumerable<T> Select<T>(this IDataReader reader, Func<IDataReader, T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }

        public static List<Entry> ReadEntrys(this IDataReader reader)
        {
            var entrys = reader.Select(r => new Entry
            {
                Time = DateTime.Parse(r["Time"].ToString()),
                User = r["User"].ToString(),
                Type = (SessionSwitchReason)Enum.Parse(typeof(SessionSwitchReason), r["Type"].ToString()),
            }).ToList();

            return entrys;
        }

        public static DateTime? ReadDate(this IDataReader reader)
        {
            DateTime? date = reader.Select(r =>
                DateTime.Parse(r["Time"].ToString())).FirstOrDefault();

            return date;
        }

        public static List<DateTime> ReadDates(this IDataReader reader)
        {
            return reader.Select(r =>
                DateTime.Parse(r["Time"].ToString())).ToList();
        }
    }

    
}
