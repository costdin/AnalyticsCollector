using Analytics.Database.ConnectionFactories;
using Analytics.Database.QueryBuilders;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Database
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public AnalyticsRepository(
            IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task SaveSession(AnalyticsEntry entry)
        {
            var client = _connectionFactory.GetClient();
            var session = ToSession(entry);

            var response = await client.IndexAsync(
                session,
                s => s.Id(session.SessionId));
        }

        public async Task SaveEvents(string sessionId, AnalyticsEvent[] events)
        {
            var client = _connectionFactory.GetClient();
            var elasticEvents = events
                .Select(e => ToEvent(sessionId, e))
                .ToArray();

            var response = await client.UpdateAsync<ElasticAnalyticsSession>(sessionId,
                q => q.Script(sn => sn.Source("if (ctx._source.events == null) { ctx._source.events = params.events; } else { ctx._source.events.addAll(params.events); }")
                                      .Params(p => p.Add("events", elasticEvents)))
                      .RetryOnConflict(3));
            
            if (!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task DeleteIndex(string index)
        {
            var client = _connectionFactory.GetClient();

            var response = await client.DeleteIndexAsync(index);

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task CreateIndex(string index)
        {
            var client = _connectionFactory.GetClient();

            var response = await client.CreateIndexAsync(index, r => 
                r.Mappings(m => m.Map<ElasticAnalyticsSession>(me => me.AutoMap())));

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }
        }

        public async Task SaveBulk(IEnumerable<AnalyticsEntry> entry)
        {
            var client = _connectionFactory.GetClient();
            var sessions = entry.Select(ToSession);
            var tcs = new TaskCompletionSource<bool>();

            var observer = client.BulkAll(sessions,
                b => b.BackOffRetries(5)
                    .BackOffTime("5s")
                    .RefreshOnCompleted(true)
                    .MaxDegreeOfParallelism(4)
                    .Size(1000));

            observer.Subscribe(new BulkAllObserver(
                    onNext: (b) => { Console.Write("*"); },
                    onError: (e) => { throw e; },
                    onCompleted: () => tcs.SetResult(true)));

            await tcs.Task;
        }

        private ElasticAnalyticsEvent ToEvent(string sessionId, AnalyticsEvent analyticsEvent)
        {
            return new ElasticAnalyticsEvent
            {
                SessionId = sessionId,
                ClientIpAddress = analyticsEvent.ClientIpAddress.ToString(),
                ElementId = analyticsEvent.ElementId,
                ElementClasses = analyticsEvent.ElementClasses,
                ElementHref = analyticsEvent.ElementHref,
                ElementText = analyticsEvent.ElementText,
                ElementType = analyticsEvent.ElementType,
                EventTime = analyticsEvent.EventTime.ToUnixTimeMilliseconds(),
                EventType = analyticsEvent.EventType,
                X = analyticsEvent.X,
                Y = analyticsEvent.Y,
                Path = analyticsEvent.Path,
                Query = analyticsEvent.Query,
                Fragment = analyticsEvent.Fragment
            };
        }

        private ElasticAnalyticsSession ToSession(AnalyticsEntry entry)
        {
            return new ElasticAnalyticsSession
            {
                SiteId = entry.SiteId,
                SessionId = entry.SessionId,
                UserAgent = entry.UserAgent,
                Browser = entry.Browser,
                CustomProperties = entry.CustomProperties?
                    .Select(ToCustomProperty)
                    .ToArray(),
                Events = entry.Events?
                    .Select(e => ToEvent(entry.SessionId, e))
                    .ToArray()
            };
        }

        private CustomProperty ToCustomProperty(KeyValuePair<string, string> kvp)
        {
            return new CustomProperty
            {
                Key = kvp.Key,
                Value = kvp.Value
            };
        }
    }
}
