using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomAdministration_WPF
{
    public class RentTable
    {
        public const int maxClass = 14;
        public List<Rent> Rents;

        public RentTable(List<Rent> list)
        {
            Rents = list;          
        }

        //Get from time
        public List<Rent> GetFromDateClass(DateTime date, int c)
        {
            List<Rent> list = new List<Rent>();
            foreach (Rent r in Rents)
            {
                if (r.Time.Include(date, c)) list.Add(r);
            }
            return list;
        }
        public Rent GetRentFromDateClass(DateTime date, int c)
        {
            foreach (Rent r in Rents)
                if (r.Time.Include(date, c)) return r;
            return null;
        }
        public List<Rent> GetFromDate(DateTime date)
        {
            List<Rent> list = new List<Rent>();
            foreach (Rent r in Rents)
            {
                if (r.Time.Include(date)) list.Add(r);
            }
            return list;
        }
        public List<Rent> GetFromWeek(DateTime date)
        {
            List<Rent> list = new List<Rent>();
            TimeSpan day = new TimeSpan(1, 0, 0, 0);

            list.AddRange(GetFromDate(date));

            for (int i = 0; i < 6; ++i)
            {
                date += day;
                list.AddRange(GetFromDate(date));
            }
            return list;
        }

        public bool QuiteFreeTime(DateTime date, int c)
        {
            for (int i=0;i<6;++i)
                if (c >= RentTime.typicalClassRent[i, 0] && c <= RentTime.typicalClassRent[i, 1])
                {
                    for (int j = RentTime.typicalClassRent[i, 0]; j < RentTime.typicalClassRent[i, 1]; ++j)
                        if (GetRentFromDateClass(date, j) != null) return false;
                    return true;
                }
            return false;
        }
        //public List<Rent> GetTableFromDate(DateTime date)
        //{
        //    List<Rent> table = new List<Rent>();
        //    List<Rent> unTable = new List<Rent>();

        //    for (int i = 0; i < maxClass; ++i) table.Add(null);

        //    for (int i = 1; i < maxClass; ++i)
        //    {
        //        List<Rent> list = GetFromDateClass(date, i);
        //        if (list.Count == 0) continue;
        //        table[i] = list[0];

        //        list.RemoveAt(0);
        //        unTable.AddRange(list);
        //    }

        //    table.AddRange(unTable.Distinct().ToList());

        //    return table;
        //}

        public Rent CheckMyTime()
        {
            DateTime date = RentTime.FirstDate;
            TimeSpan days = new TimeSpan(1, 0, 0, 0);

            if (DateTime.Now > date) date = DateTime.Now;

            while (date < RentTime.LastDate)
            {
                for (int i = 1; i <= maxClass; ++i)
                {
                    List<Rent> list = GetFromDateClass(date, i);

                    if (list.Count > 1) return list[0];
                }
                date += days;
            }

            return null;
        }

        //Rent Control
        public void MoveRentToFirst(int rId)
        {
            Rent r = GetRent(rId);
            if (r == null) return;

            if (Rents.Contains(r)) Rents.Remove(r);
            Rents.Insert(0, r);
        }
        public Rent GetRent(int rId)
        {
            foreach (Rent r in Rents)
                if (r.rId == rId) return r;
            return null;
        }

        public bool Contains(int rId)
        {
            foreach (Rent r in Rents)
                if (r.rId == rId) return true;
            return false;
        }
        public Rent Add(int rId)
        {
            Rent r = DatabaseLinker.GetRent(rId); if (r == null) return new Rent();
            if (Contains(rId)) return null;

            Rents.Add(r);

            Rent rr = CheckMyTime();
            if (rr == null) return null;

            Rents.Remove(r);
            return rr;
        }
        public void Remove(Rent r)
        {
            if (Rents.Contains(r)) Rents.Remove(r);
        }
        public void Remove(int rId)
        {
            foreach (Rent r in Rents)
                if (r.rId == rId) Rents.Remove(r);
        }
 
        //Get From classroom and building
        public List<Rent> GetListClassroom(int cId)
        {
            List<Rent> list = new List<Rent>();
            foreach (Rent r in Rents)
                if (r.cId == cId) list.Add(r);
            return list;
        }
        public Rent GetClassroom(int cId)
        {
            foreach (Rent r in Rents)
                if (r.cId == cId) return r;
            return null;
        }
        public int CntRentsInBuilding(int bId)
        {
            int cnt = 0;
            foreach (Rent r in Rents)
                if (bId == Classroom.CId2BId(r.cId)) ++cnt;
            return cnt;

        }

        //ToString
        public string Display()
        {
            string s="";
            foreach (Rent r in Rents)
                if (null != r) s += r.Display() + "\r\n"; else s += "null \r\n";
            return s;
        }

        
    }
}
