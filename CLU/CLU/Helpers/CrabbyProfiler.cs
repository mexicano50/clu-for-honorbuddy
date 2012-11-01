using System;
using System.Collections.Generic;
using System.Linq;

namespace CLU.Helpers
{
    public class CrabbyProfiler
    {
        public List<Run> Runs;
        public List<ReportItem> Reports;

        private static CrabbyProfiler instance;

        public static CrabbyProfiler Instance
        {
            get
            {
                return instance ?? (instance = new CrabbyProfiler());
            }
        }

        public CrabbyProfiler()
        {
            Runs = new List<Run>();
            Reports = new List<ReportItem>();
        }

        public void EndLast()
        {
            Runs.Last().End();
        }

        public void Report()
        {
            foreach (Run r in Runs)
            {
                if (NeedStartReport(r.Name))
                {
                    Reports.Add(new ReportItem(r));
                }
                else
                    getReport(r).UpdateReportItem(r);
            }

            foreach (ReportItem r in Reports)
            {
                r.CloseReportItem();
                Styx.Common.Logging.Write(r.Name + "\t" +
                        r.highTime + "\t" +
                        r.lowTime + "\t" +
                        r.averageTime + "\t" +
                        r.totalCalls + "\t" +
                        r.aggregateTime);
            }
        }

        public bool NeedStartReport(string name)
        {
            return !Reports.Any(r => r.Name == name);
        }

        public ReportItem getReport(Run r)
        {
            return (from ReportItem rep in Reports
                    where rep.Name == r.Name
                    select rep).FirstOrDefault();
        }
    }

    public class ReportItem
    {
        public string Name;
        public double totalCalls = 0;
        public double aggregateTime = 0;
        public double averageTime = 0;
        public double highTime = 0;
        public double lowTime = 0;

        public ReportItem(Run r)
        {
            Name = r.Name;
            totalCalls++;
            aggregateTime += r.totalMs;
            lowTime = r.totalMs;
            highTime = r.totalMs;
        }

        public void UpdateReportItem(Run r)
        {
            totalCalls++;
            aggregateTime += r.totalMs;

            if (r.totalMs < lowTime)
                lowTime = r.totalMs;

            if (r.totalMs > highTime)
                highTime = r.totalMs;
        }

        public void CloseReportItem()
        {
            averageTime = aggregateTime / totalCalls;
        }
    }

    public class Run
    {
        public string Name;
        public DateTime start;
        public DateTime finish;
        public double totalMs;

        public Run(string name)
        {
            Name = name;
            start = DateTime.Now;
            Styx.Common.Logging.Write("New Run created: " + name);
        }

        public void End()
        {
            finish = DateTime.Now;
            totalMs = finish.Subtract(start).Milliseconds;
            Styx.Common.Logging.Write("Ended Run: " + Name + " " + totalMs);
        }
    }
}