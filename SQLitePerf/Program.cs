using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace SQLitePerf
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                File.Delete("MyDatabase.sqlite");
                var times = 100;

                using (var conn = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;"))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "create table t1(a int, b varchar(200))";
                        cmd.ExecuteNonQuery();
                    }

                    Run(conn, times, "Default Config");
                    Console.WriteLine("-----------");

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "drop table t1";
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var conn = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;"))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "create table t1(a int, b varchar(200))";
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA synchronous = OFF";
                        cmd.ExecuteNonQuery();
                    }

                    Run(conn, times, "Custom Config");
                }

                Console.WriteLine("-----------");
                RunInMemoryDic(times);

                Console.ReadLine();
            }
        }

        static void Run(SQLiteConnection conn, int times, string description)
        {
            Console.WriteLine(description);
            Console.WriteLine("Times: " + times);
            Console.WriteLine();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"insert into t1 values ({i}, '{i}')";
                    cmd.ExecuteNonQuery();
                }
            }
            sw.Stop();
            Console.WriteLine($"inserts: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            for (int i = 0; i < times; i++)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"update t1 set b = '{i + 1}' where a = {i}";
                    cmd.ExecuteNonQuery();
                }
            }
            sw.Stop();
            Console.WriteLine($"updates: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            for (int i = 0; i < times; i++)
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"delete from t1 where a = {i}";
                    cmd.ExecuteNonQuery();
                }
            }
            sw.Stop();
            Console.WriteLine($"deletes: {sw.ElapsedMilliseconds} ms");
        }

        static void RunInMemoryDic(int times)
        {
            Console.WriteLine("InMemory Dictionary");
            Console.WriteLine("Times: " + times);
            Console.WriteLine();

            var data = new Dictionary<int, string>();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                data.Add(i, i.ToString());
            }
            sw.Stop();
            Console.WriteLine($"inserts: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            for (int i = 0; i < times; i++)
            {
                data[i] = (i + 1).ToString();
            }
            sw.Stop();
            Console.WriteLine($"updates: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            for (int i = 0; i < times; i++)
            {
                data.Remove(i);
            }
            sw.Stop();
            Console.WriteLine($"deletes: {sw.ElapsedMilliseconds} ms");
        }
    }
}
