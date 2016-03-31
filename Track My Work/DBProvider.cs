using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace Track_My_Work
{
    class DBProvider
    {
        /// <summary>
        /// select date from mysql date string in yyyyMMddhhmmss format
        /// </summary>
        private const string OrderedDateStr = "substr(TIME,7,4)||substr(TIME,4,2)||substr(TIME,1,2)||substr(TIME,12,2)||substr(TIME,15,2)||substr(TIME,18,2)";
        private const string DBName = "tmw.sqlite";
        /// <summary>
        /// work week days
        /// </summary>
        private static readonly List<DayOfWeek> WorkDayOfWeek = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + DBName;
        private static readonly string CS = string.Format("data source={0};version=3;new=False;datetimeformat=CurrentCulture;PRAGMA journal_mode=WAL;Pooling=True;", Path);
        
        public static void CreateDBIfNotExist()
        {
            if (File.Exists(Path)) return;

            SQLiteConnection.CreateFile(Path);

            using (var sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                const string sql = @"CREATE TABLE LOG(
                                      Id INTEGER PRIMARY KEY,
                                      Time STRING,
                                      Type INTEGER,
                                      User TEXT(100)
                                    ); ";
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void Insert(DateTime time, SessionSwitchReason reason, string user)
        {
            var queryString = string.Format(@"INSERT INTO LOG (Time, Type, User) 
                                            VALUES (""{0}"", {1}, ""{2}""); ", time.ToString("dd.MM.yyyy HH:mm:ss"), (int)reason, user);            

            using (var sqlite = new SQLiteConnection(CS))
            {
                using (var command = new SQLiteCommand(queryString, sqlite))
                {
                    command.Connection.Open();
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        public static DataTable ReadAll()
        {
            var dt = new DataTable();

            using (var sqlite = new SQLiteConnection(CS))
            {
                const string sql = @"SELECT *,"+ OrderedDateStr +"as str FROM LOG";

                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();
                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        dt.Load(dr);
                    }
                }
                return dt;
            }
        }


        public static IEnumerable<DateTime> StartTimes()
        {
            using (var sqlite = new SQLiteConnection(CS))
            {
                const string sql = "SELECT TIME FROM LOG";
                IEnumerable<DateTime> mins;
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    List<DateTime> data;
                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        data = dr.ReadDates();
                    }
                    //grouped by day at work week start times 
                    var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                    //consider that work day start and finish at 5am 
                    mins = gropedDates.Select(d => d.Where(time => time.TimeOfDay > new TimeSpan(0, 5, 0)).Min(t => t)).ToList();
                    sqlite.Close();
                }
                return mins;
            }
        }
        public static IEnumerable<DateTime> EndTimes()
        {
            using (var sqlite = new SQLiteConnection(CS))
            {
                
                const string sql = "SELECT TIME FROM LOG";
                IEnumerable<DateTime> maxs;
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    List<DateTime> data;
                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        data = dr.ReadDates();
                    }

                    //grouped by day at work week start times 
                    var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                    maxs = gropedDates.Select(d => d.Max(t => t)).ToList();
                }
                return maxs;
            }
        }
        /// <summary>
        /// Read all info about day
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static List<Entry> ReadDay(DateTime day)
        {
            //consider that work day start and finish at 5am 
            var start = day.Date.AddHours(5);
            day = day.AddDays(1);
            var end = day.Date.AddHours(5);
            
            using (var sqlite = new SQLiteConnection(CS))
            {
                var sql = string.Format(@"SELECT * FROM LOG  WHERE " + OrderedDateStr + " > \"{0}\" AND " + OrderedDateStr + " < \"{1}\" ", start.ToSqlOrderedString(), end.ToSqlOrderedString());

                List<Entry> data;
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();
                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        data = dr.ReadEntrys();
                    }
                }
                return data;
            }
        }
        public static DateTime? GetLogStartDay()
        {
            using (var sqlite = new SQLiteConnection(CS))
            {
                const string sql = "SELECT TIME, MIN(" + OrderedDateStr + ") as STR FROM LOG ORDER BY STR";

                DateTime? data;
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();

                    command.ExecuteNonQuery();
                    using (var dr = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        data = dr.ReadDate();
                    }
                }
                return data;
            }
        }

    }
}
