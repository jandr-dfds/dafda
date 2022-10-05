using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration
{
    internal class ConfigurationKeys : IEnumerable<ConfigurationKeys.Key>
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
            { BootstrapServers, true },
            { GroupId, true },
            EnableAutoCommit,
            AllowAutoCreateTopics,
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

        private readonly List<Key> _keys = new();

        public string[] Required => _keys.Where(x => x.Required).Select(x => x.ToString()).ToArray();

        public void Add(string key, bool required = false)
        {
            var keyWrapper = new Key(key, required);

            _keys.Add(keyWrapper);
        }

        public IEnumerator<Key> GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class Key
        {
            private readonly string _key;

            public Key(string key, bool required = false)
            {
                _key = key;
                Required = required;
            }

            public bool Required { get; }

            public override string ToString()
            {
                return _key;
            }

            public static implicit operator string(Key key)
            {
                return key.ToString();
            } 
        }
    }
}