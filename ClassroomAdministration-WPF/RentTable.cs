﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomAdministration_WPF
{
    public class RentTable
    {
        public const int maxClass = 15;
        public  List<Rent> Rents;

        public RentTable(List<Rent> list)
        {
            Rents = list;          
        }

        public List<Rent> GetFromDateClass(DateTime date, int c)
        {
            List<Rent> list = new List<Rent>();
            foreach (Rent r in Rents)
            {
                if (r.Time.Include(date, c)) list.Add(r);
            }
            return list;
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
        public Rent CheckMe()
        {
            DateTime date = RentTime.FirstDate;
            TimeSpan days = new TimeSpan(8, 0, 0, 0);

            for (int ii = 0; ii < 14; ++ii)
            {
                for (int i = 1; i < maxClass; ++i)
                {
                    List<Rent> list = GetFromDateClass(date, i);
                    if (list.Count > 1) return list[0];
                }
                date += days;
            }

            return null;
        }

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

        public List<Rent> GetClassroom(int cId)
        {
            List<Rent> list = new List<Rent>();
            foreach (Rent r in Rents)
                if (r.cId == cId) list.Add(r);
            return list;
        }
        public int CntRentsInBuilding(int bId)
        {
            int cnt = 0;
            foreach (Rent r in Rents)
                if (bId == Classroom.CId2BId(r.cId)) ++cnt;
            return cnt;

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

            Rent rr = CheckMe();
            if (rr == null) return null;
            
            Rents.Remove(r);
            return rr;
        }
        public void Remove(Rent r)
        {
            if (Rents.Contains(r)) Rents.Remove(r);
        }

        public string Display()
        {
            string s="";
            foreach (Rent r in Rents)
                if (null != r) s += r.Display() + "\r\n"; else s += "null \r\n";
            return s;
        }

        
    }
}
