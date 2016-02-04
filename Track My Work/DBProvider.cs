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
        private static readonly List<DayOfWeek> WorkDayOfWeek = new List<DayOfWeek>() {DayOfWeek.Friday, DayOfWeek.Monday, DayOfWeek.Thursday, DayOfWeek.Saturday, DayOfWeek.Wednesday};
        private const string DBName = "tmw.sqlite";
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + DBName;
        private static readonly string CS = string.Format("data source={0};version=3;new=False;datetimeformat=CurrentCulture;PRAGMA journal_mode=WAL;Pooling=True;", Path);
        
        public static void CreateDB()
        {
            if (File.Exists(Path)) return;

            SQLiteConnection.CreateFile(Path);

            using (var sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                const string sql = @"CREATE TABLE LOG(
                                      Id INTEGER PRIMARY KEY,
                                      Time datetime,
                                      Type INTEGER,
                                      User TEXT(100)
                                    ); ";
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();
                sqlite.Close();
            }
        }
        public static void Insert(DateTime time, SessionSwitchReason reason, string user)
        {
            SQLiteConnection.ClearAllPools();

            var queryString = string.Format(@"INSERT INTO LOG (Time, Type, User) 
                                            VALUES (""{0}"", {1}, ""{2}""); ", time, (int)reason, user);            

            using (var sqlite = new SQLiteConnection(CS))
            {
                using (var command = new SQLiteCommand(queryString, sqlite))
                {
                    command.Connection.Open();
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }
        public static DataTable ReadAll()
        {
            SQLiteConnection.ClearAllPools();
            var dt = new DataTable();

            using (var sqlite = new SQLiteConnection(CS))
            {
                var sql = @"SELECT * FROM LOG";

                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    var dr = command.ExecuteReader(CommandBehavior.CloseConnection);

                    dt.Load(dr);
                    sqlite.Close();
                }
                return dt;
            }
        }


        public static IEnumerable<DateTime> StartTimes()
        {
            SQLiteConnection.ClearAllPools();
            IEnumerable<DateTime> mins = new List<DateTime>();
            using (var sqlite = new SQLiteConnection(CS))
            {
                var sql = string.Format(@"SELECT TIME FROM LOG");
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    var data = dr.ReadDates();

                    var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                    mins = gropedDates.Select(d => d.Where(time => time.TimeOfDay > new TimeSpan(0, 5, 0)).Min(t => t)).ToList();
                    sqlite.Close();
                }
                return mins;
            }
        }
        public static IEnumerable<DateTime> EndTimes()
        {
            SQLiteConnection.ClearAllPools();
            IEnumerable<DateTime> maxs = new List<DateTime>();

            using (var sqlite = new SQLiteConnection(CS))
            {
                
                var sql = string.Format(@"SELECT TIME FROM LOG");
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    var data = dr.ReadDates();

                    var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                    maxs = gropedDates.Select(d => d.Max(t => t)).ToList();
                    sqlite.Close();
                }
                return maxs;
            }
        }

        public static List<Entry> ReadDay(DateTime day)
        {
            SQLiteConnection.ClearAllPools();
            List<Entry> data = new List<Entry>();
            var start = day.Date.AddHours(5);
            day = day.AddDays(1);
            var end = day.Date.AddHours(5);
            
            using (var sqlite = new SQLiteConnection(CS))
            {
                
                var sql = string.Format(@"SELECT * FROM LOG  WHERE TIME > ""{0}"" AND TIME < ""{1}"" ", start, end);
               
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();
                    command.ExecuteNonQuery();

                    var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    data = dr.ReadEntrys();

                    sqlite.Close();
                }
               

                return data;
            }
        }
        public static DateTime? GetLogStartDay()
        {
            SQLiteConnection.ClearAllPools();
            DateTime? data = null;
            using (var sqlite = new SQLiteConnection(CS))
            {
                const string sql =
                        "SELECT TIME, MIN(substr(TIME,7,4)||substr(TIME,4,2)||substr(TIME,1,2)) as STR FROM LOG  ORDER BY STR";
                using (var command = new SQLiteCommand(sql, sqlite))
                {
                    sqlite.Open();

                    command.ExecuteNonQuery();

                    var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    data = dr.ReadDate();

                    sqlite.Close();
                }
               

                return data;
            }
        }

    }
}
