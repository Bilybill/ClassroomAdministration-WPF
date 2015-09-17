using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomAdministration_WPF
{
    public class RentTime
    {
        private DateTime startDate, endDate;
        private int cycDays, startClass, endClass, weekDay;

        public DateTime StartDate { get { return startDate; } }
        public int StartClass { get { return startClass; } }
        public int KeepClass { get { return endClass - startClass + 1; } }
        public int WeekDay { get { return weekDay; } }
        public bool OnceActivity { get { Console.WriteLine(cycDays); return cycDays == 0; } }
        public bool BeenOver { get { return endDate < DateTime.Now; } }

        public RentTime(string stD, string edD, int cycD, int stC, int edC)
        {
            DateTime.TryParse(stD, out startDate);
            DateTime.TryParse(edD, out endDate);
           
            TimeSpan ts = startDate - FirstDate;
            weekDay = ts.Days % 7; if (weekDay < 0) weekDay += 7;

            cycDays = cycD;
            startClass = stC;
            endClass = edC;

            endDate += new TimeSpan(0, 23, 0, 0);
        }
        public RentTime(DateTime stD, DateTime edD, int cycD, int stC, int edC)
        {
            startDate = stD;
            endDate = edD;

            TimeSpan ts = startDate - FirstDate;
            weekDay = ts.Days % 7; if (weekDay < 0) weekDay += 7;

            cycDays = cycD;
            startClass = stC;
            endClass = edC;

            endDate += new TimeSpan(0, 23, 0, 0);
        }

        public bool Include(DateTime date, int c = -1)
        {
            

            if (date < startDate || date > endDate) return false;
            if (c != -1) if (c < startClass || c > endClass) return false;

            if (cycDays == 0) return true;

            TimeSpan ts = date - startDate;
            if (0 != ts.Days % cycDays) return false;

            return true;
        }

        //ToString in DatabaseLinker
        public override string ToString()
        {
            string s = "";
            s += "\'" + startDate.ToString("yyyy-MM-dd") + "\', ";
            s += "\'" + endDate.ToString("yyyy-MM-dd") + "\', ";
            s += cycDays + ", " + startClass + ", " + endClass;

            return s;
        }

        //Display 
        public string Display()
        {
            return "周" + weekDayName[weekDay] + ", 第" + startClass + "节至第" + endClass + "节";
        }

        //static items of time
        static public DateTime FirstDate = new DateTime(2015, 9, 14);
        static public DateTime LastDate = new DateTime(2016, 2, 21);
        static public int[,] typicalClassRent =
        {
            {1,2},
            {3,5},
            {6,7},
            {8,9},
            {10,11},
            {12,14}
        };
        static public string[] StringClassTime =
        {
            "08:00~08:45",
            "08:50~09:35",
            "09:50~10:35",
            "10:40~11:25",
            "11:30~12:15",
            "13:30~14:15",
            "14:20~15:05",
            "15:20~16:05",
            "16:10~16:55",
            "17:05~17:50",
            "17:55~18:40",
            "19:20~20:05",
            "20:10~20:55",
            "21:00~21:45"
        };
        static public string[] weekDayName = { "一", "二", "三", "四", "五", "六", "日" };
        

    }
}
