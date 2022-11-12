using Analytics.Database;
using Analytics.Database.ConnectionFactories;
using Analytics.Database.QueryBuilders;
using Elasticsearch.Net;
using NodaTime;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace FunnelFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory(new[] { "http://192.168.1.88/elasticsearch" }, "user", "bitnami", "latest_analytics", Log, false);
            var queryBuilder = new FunnelQueryBuilder();
            var repo = new FunnelRepository(connectionFactory, queryBuilder);

            var analyticsRepo = new AnalyticsRepository(connectionFactory);

            analyticsRepo.DeleteIndex("latest_analytics").Wait();
            analyticsRepo.CreateIndex("latest_analytics").Wait();

            CreateData(analyticsRepo);

            int cnt = 0;
            while (cnt++ < 30)
            {
                //var xxxx = repo.FunnelAnalysis(
                //    new[]
                //    {
                //        new Funnel(null, null, "/India", "pagehit", null),
                //        new Funnel(null, null, "/Maro", "pagehit", null),
                //        new Funnel(null, null, "/", "pagehit", null)
                //    },
                //    "analytics_nested2");

                var xxxx = repo.FunnelAnalysis(
                    new[]
                    {
                        new Funnel(null, null, "/India", "pagehit", null),
                        new Funnel(null, null, "/Maro", "pagehit", null),
                        new Funnel(null, null, "/", "pagehit", null)
                    },
                    "analytics_nested2");

                var funnels = xxxx.Result
                    .Select(n => n.ToString())
                    .Aggregate((a, b) => $"{a} => {b}");

                Console.WriteLine(funnels);
            }

            Console.WriteLine("Done");
        }

        static void Log(IApiCallDetails s)
        {
            if (s.RequestBodyInBytes?.Any() == true)
            {
                var query = Encoding.UTF8.GetString(s.RequestBodyInBytes);
                Console.WriteLine($"Sent query:\n{query}");
            }

            if (s.ResponseBodyInBytes?.Any() == true)
            {
                var response = Encoding.UTF8.GetString(s.ResponseBodyInBytes);
                Console.WriteLine($"Received response:\n{response}");
            }
        }

        static void CreateData(AnalyticsRepository repo)
        {
            var sessions = Enumerable
                .Range(1, 500000)
                .Select(i => CreateEntry(50));

            var t = repo.SaveBulk(sessions);
            t.Wait();
        }

        static Random rand = new Random();
        static AnalyticsEntry CreateEntry(int eventCount)
        {
            var ip = new IPAddress(new[] { (byte)(rand.Next() % 256), (byte)(rand.Next() % 256), (byte)(rand.Next() % 256), (byte)(rand.Next() % 256) });

            return new AnalyticsEntry
            {
                Browser = "Firefox",
                SessionId = rand.NextDouble().ToString() + rand.NextDouble().ToString() + rand.NextDouble().ToString() + rand.NextDouble().ToString(),
                SiteId = "Site123",
                UserAgent = "Firefox UserAgent",
                Events = Enumerable.Range(1, eventCount).Select(i => CreateEvent(ip, i)).ToArray()
            };
        }

        static Instant lastEvent = Instant.FromDateTimeUtc(DateTime.UtcNow);
        static AnalyticsEvent CreateEvent(IPAddress ip, int i)
        {
            lastEvent = lastEvent.Plus(Duration.FromMinutes(1));

            return new AnalyticsEvent
            {
                ClientIpAddress = ip,
                ElementId = $"div{i}",
                EventTime = lastEvent,
                EventType = "click",
                Fragment = "",
                Path = "/page",
                Query = $"id={i}",
                X = i * 10,
                Y = i * 10
            };
        }
    }
}
