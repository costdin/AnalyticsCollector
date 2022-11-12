using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics.Database
{
    public class Funnel
    {
        public Range XRange { get; }
        public Range YRange { get; }
        public string Path { get; }
        public string EventType { get; }
        public string ElementId { get; }

        public Funnel(
            Range xRange,
            Range yRange,
            string path,
            string eventType,
            string elementId)
        {
            XRange = xRange;
            YRange = yRange;
            Path = path;
            EventType = eventType;
            ElementId = elementId;
        }
    }

    public class Range
    {
        public int Start { get; }
        public int End { get; }

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
