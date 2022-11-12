using Nest;
using System;
using System.Collections.Generic;

namespace Analytics.Database
{
    [ElasticsearchType(Name = "analytics_session")]
    public class ElasticAnalyticsSession
    {
        [Keyword]
        public string SiteId { get; set; }

        [Keyword]
        public string SessionId { get; set; }

        [Text]
        public string UserAgent { get; set; }

        [Keyword]
        public string Browser { get; set; }

        [Object]
        public CustomProperty[] CustomProperties { get; set; }

        [Nested]
        public ElasticAnalyticsEvent[] Events { get; set; }
    }

    public class ElasticAnalyticsEvent
    {
        [Keyword]
        public string SessionId { get; set; }

        [Date(Format = "epoch_millis")]
        public long EventTime { get; set; }

        [Ip]
        public string ClientIpAddress { get; set; }

        [Keyword]
        public string ElementId { get; set; }

        [Keyword]
        public string ElementType { get; set; }

        [Keyword]
        public string EventType { get; set; }

        [Keyword]
        public string[] ElementClasses { get; set; }

        [Text]
        public string ElementHref { get; set; }

        [Text]
        public string ElementText { get; set; }

        [Number(NumberType.Integer)]
        public int? X { get; set; }

        [Number(NumberType.Integer)]
        public int? Y { get; set; }

        [Text]
        public string Path { get; set; }

        [Text]
        public string Query { get; set; }

        [Text]
        public string Fragment { get; set; }
    }

    public class CustomProperty
    {
        [Keyword]
        public string Key { get; set; }

        [Text]
        public string Value { get; set; }
    }
}
