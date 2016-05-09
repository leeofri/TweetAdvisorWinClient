using System;
using System.Collections.Generic;

namespace StockAnalyzer.Site.Models
{
    public class UserOptions
    {
        public int StocksNumber { get; set; }

        public int DaysNumber { get; set; }

        public int ClusterNumber { get; set; }

        public bool Open { get; set; }

        public bool Close { get; set; }

        public bool High { get; set; }

        public bool Low { get; set; }

    }
    public class CandidateData
    {
        public string startDate { get; set; }

        public string endDate { get; set; }

    }

    public class DayResult
    {
        public float HC { get; set; }

        public float DT { get; set; }

        public float TC { get; set; }

        public float BS { get; set; }

        public float JK { get; set; }

        public string PredictionDate { get; set; }
    }

    public class FullResult
    {
        public DayResult[] CandidatesResults { get; set; }
    }
}
