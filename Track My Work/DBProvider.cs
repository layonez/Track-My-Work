using System;
using System.Data.SQLite;
using Microsoft.Win32;
using System.Data;
using System.Collections.Generic;
using Track_My_Work;
using System.Linq;

namespace Track_My_Work
{
    class DBProvider
    {
        private static readonly List<DayOfWeek> WorkDayOfWeek = new List<DayOfWeek>() {DayOfWeek.Friday, DayOfWeek.Monday, DayOfWeek.Thursday, DayOfWeek.Saturday, DayOfWeek.Wednesday};
        private const string DBName = "tmw.sqlite";
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + DBName;
        private static readonly string CS = string.Format("data source={0};version=3;new=False;datetimeformat=CurrentCulture", Path);        
        public static void CreateDB()
        {
            if (System.IO.File.Exists(Path)) return;

            SQLiteConnection.CreateFile(Path);

            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
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
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            SQLiteConnection sqlite;

            var queryString = string.Format(@"INSERT INTO LOG (Time, Type, User) 
                                            VALUES (""{0}"", {1}, ""{2}""); ", time, (int)reason, user);            

            using (sqlite = new SQLiteConnection(CS))
            {
                using (var command = new SQLiteCommand(queryString, sqlite))
                {
                    command.Connection.Open();
                    command.ExecuteScalar();
                    command.Connection.Close();
                }
            }
        }
        public static DataTable ReadAll()
        {
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                var sql = @"SELECT * FROM LOG";
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();
                
                var dr = command.ExecuteReader(CommandBehavior.CloseConnection);

                var dt = new DataTable();
                dt.Load(dr);
                sqlite.Close();

                return dt;
            }
        }


        public static IEnumerable<DateTime> StartTimes()
        {
            System.Data.SQLite.SQLiteConnection.ClearAllPools();

            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                var sql = string.Format(@"SELECT TIME FROM LOG");
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();

                var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                var data = dr.ReadDates();

                var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                var mins = gropedDates.Select(d=>d.Where(time=>time.TimeOfDay > new TimeSpan(0,5,0)).Min(t=>t));
                sqlite.Close();

                return mins;
            }
        }
        public static IEnumerable<DateTime> EndTimes()
        {
            System.Data.SQLite.SQLiteConnection.ClearAllPools();

            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                var sql = string.Format(@"SELECT TIME FROM LOG");
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();

                var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                var data = dr.ReadDates();

                var gropedDates = data.Where(d => WorkDayOfWeek.Contains(d.DayOfWeek)).GroupBy(d => d.Date);
                var maxs = gropedDates.Select(d => d.Max(t => t));
                sqlite.Close();

                return maxs;
            }
        }

        public static List<Entry> ReadDay(DateTime day)
        {
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            var start = day.Date;
            day = day.AddDays(1);
            var end = day.Date;

            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();
                var sql = string.Format(@"SELECT * FROM LOG WHERE TIME > ""{0}"" AND TIME < ""{1}"" ", start, end);
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();

                var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                var data = dr.ReadEntrys();

                sqlite.Close();

                return data;
            }
        }
        public static DateTime? GetLogStartDay()
        {
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            SQLiteConnection sqlite;
            using (sqlite = new SQLiteConnection(CS))
            {
                sqlite.Open();

                const string sql =
                    "SELECT TIME, MIN(substr(TIME,7,4)||substr(TIME,4,2)||substr(TIME,1,2)) as STR FROM LOG  ORDER BY STR";
                var command = new SQLiteCommand(sql, sqlite);
                command.ExecuteNonQuery();

                var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                var data = dr.ReadDate();

                sqlite.Close();

                return data;
            }
        }

    }
}
