using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    class Lgr
    {
        public static int dayOfMonth;

        public static void Start()
        {
            int lastDay = 0;
            dayOfMonth = DateTime.Now.Day;
            if (File.Exists("start.txt"))
            {
                using (StreamReader r = File.OpenText("start.txt"))
                {
                    lastDay = int.Parse(r.ReadLine());
                }
            }
            if(lastDay != dayOfMonth)
            {
                using (StreamWriter w = File.CreateText(dayOfMonth.ToString() + ".txt"))
                {
                    WriteLog("DEBUG", "New day starting", w);
                }
                using (StreamWriter w = File.CreateText("trades_" + dayOfMonth.ToString() + ".txt"))
                {
                    w.WriteLine("");
                }
            }
            if (!File.Exists(dayOfMonth.ToString() + ".txt")) // make sure the file exists and create if not
            {
                using (StreamWriter w = File.CreateText(dayOfMonth.ToString() + ".txt"))
                {
                    WriteLog("DEBUG", "New day starting", w);
                }
            }
            if (!File.Exists("trades_" + dayOfMonth.ToString() + ".txt")) // make sure the file exists and create if not
            {
                using (StreamWriter w = File.CreateText("trades_" + dayOfMonth.ToString() + ".txt"))
                {
                    w.WriteLine("");
                }
            }
            using (StreamWriter w = File.CreateText("start.txt"))
            {
                WriteDay(dayOfMonth.ToString(), w);
            }
        }

        public static double GetTradeValToday()
        {
            double val = 0.0;
            string line;
            
            System.IO.StreamReader file =
               new System.IO.StreamReader("trades_" + dayOfMonth.ToString() + ".txt");
            while ((line = file.ReadLine()) != null)
            {
                String[] elements = Regex.Split(line, ";");
                if (elements.Length == 3)
                {
                    val += Convert.ToDouble(elements[1]);
                }
            }
            file.Close();

            return val;
        }

        public static void WriteTrade(double value, string trade)
        {
            using (StreamWriter w = File.AppendText("trades_" + dayOfMonth.ToString() + ".txt"))
            {
                w.WriteLine("{0};{1};{2}", DateTime.Now.ToLongTimeString(), value.ToString(), trade);
            }
        }

        public static void Log(string level, string msg)
        {
            using (StreamWriter w = File.AppendText(dayOfMonth.ToString() + ".txt"))
            {
                WriteLog(level, msg, w);
            }
        }

        public static void ReadLog()
        {
            using (StreamReader r = File.OpenText("log.txt"))
            {
                DumpLog(r);
            }
        }

        private static void WriteDay(string day, TextWriter w)
        {
            w.Write(day);
        }

        private static void WriteLog(string level, string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1} [{2}]", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString(), level);
            w.WriteLine("  ");
            w.WriteLine("  {0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        private static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
