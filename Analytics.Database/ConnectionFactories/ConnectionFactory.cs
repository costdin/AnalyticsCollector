using Elasticsearch.Net;
using Nest;
using System;
using System.Linq;

namespace Analytics.Database.ConnectionFactories
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly ElasticClient client;

        public ConnectionFactory(
            string[] nodes,
            string username,
            string password,
            string index,
            Action<IApiCallDetails> requestCompleteAction,
            bool debug = false)
        {
            var uris = nodes
                .Select(n => new Uri(n))
                .ToArray();

            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool)
                .DisableDirectStreaming(debug);

            if (debug && requestCompleteAction != null)
            {
                settings.OnRequestCompleted(requestCompleteAction);
            }

            settings = settings.BasicAuthentication(username, password);
            settings.DefaultIndex(index);

            client = new ElasticClient(settings);
        }

        public ElasticClient GetClient() => client;
    }
}
