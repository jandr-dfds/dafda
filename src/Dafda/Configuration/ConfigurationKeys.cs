using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration
{
    internal class ConfigurationKeys : IEnumerable<string>
    {
        public const string GroupId = "group.id";
        public const string EnableAutoCommit = "enable.auto.commit";
        public const string AllowAutoCreateTopics = "allow.auto.create.topics";
        public const string BootstrapServers = "bootstrap.servers";
        public const string BrokerVersionFallback = "broker.version.fallback";
        public const string ApiVersionFallbackMs = "api.version.fallback.ms";
        public const string SslCaLocation = "ssl.ca.location";
        public const string SaslUsername = "sasl.username";
        public const string SaslPassword = "sasl.password";
        public const string SaslMechanisms = "sasl.mechanisms";
        public const string SecurityProtocol = "security.protocol";

        public static readonly ConfigurationKeys Empty = new();

        public static readonly ConfigurationKeys Consumer = new()
        {
            { GroupId, true },
            EnableAutoCommit,
            AllowAutoCreateTopics,
            { BootstrapServers, true },
            BrokerVersionFallback,
            ApiVersionFallbackMs,
            SslCaLocation,
            SaslUsername,
            SaslPassword,
            SaslMechanisms,
            SecurityProtocol,
        };

        public static readonly ConfigurationKeys Producer = new()
        {
            { BootstrapServers, true },
            BrokerVersionFallback,
            ApiVersionFallbackMs,
            SslCaLocation,
            SaslUsername,
            SaslPassword,
            SaslMechanisms,
            SecurityProtocol,
        };

        private readonly List<KeyWrapper> _keys = new();
        private readonly HashSet<string> _uniqueKeys = new();

        public string[] Required => _keys.Where(x => x.Required).Select(x => x.Key).ToArray();

        public void Add(string key, bool required = false)
        {
            _keys.Add(new KeyWrapper(key, required));
            _uniqueKeys.Add(key);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _uniqueKeys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private record KeyWrapper(string Key, bool Required = false);
    }
}