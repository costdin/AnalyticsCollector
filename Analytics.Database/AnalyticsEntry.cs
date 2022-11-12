using NodaTime;
using System.Collections.Generic;
using System.Net;

namespace Analytics.Database
{
    public class AnalyticsEntry
    {
        public string SiteId { get; set; }
        public string SessionId { get; set; }
        public string UserAgent { get; set; }
        public string Browser { get; set; }
        public KeyValuePair<string, string>[] CustomProperties { get; set; }
        public AnalyticsEvent[] Events { get; set; }
    }

    public class AnalyticsEvent
    {
        public string Path { get; set; }
        public string Query { get; set; }
        public string Fragment { get; set; }
        public IPAddress ClientIpAddress { get; set; }
        public Instant EventTime { get; set; }
        public string ElementId { get; set; }
        public string ElementType { get; set; }
        public string EventType { get; set; }
        public string ElementHref { get; set; }
        public string ElementText { get; set; }
        public string[] ElementClasses { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
