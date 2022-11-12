using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyticsCollector.DTOs
{
    public class CreateSessionDto
    {
        public string SiteId { get; set; }

        [JsonConverter(typeof(ObjectToListOfKeyValuePairConverter))]
        public KeyValuePair<string, string>[] CustomProperties { get; set; }
    }

    public class AddEventsDto
    {
        public string SessionId { get; set; }
        public AnalyticsEventDto[] Events { get; set; }
    }

    public class AnalyticsEventDto
    {
        public string Path { get; set; }
        public string Query { get; set; }
        public string Fragment { get; set; }
        public Instant EventTime { get; set; }
        public string ElementId { get; set; }
        public string ElementType { get; set; }
        public string EventType { get; set; }
        public string[] ElementClasses { get; set; }
        public string ElementHref { get; set; }
        public string ElementText { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }

    public class ObjectToListOfKeyValuePairConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(KeyValuePair<string, string>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Array.Empty<KeyValuePair<string, string>>();
            }

            JObject jsonObject = JObject.Load(reader);

            return jsonObject.Properties()
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Value.ToString()))
                .ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
