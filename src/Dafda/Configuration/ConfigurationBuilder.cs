using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dafda.Configuration
{
    internal class ConfigurationBuilder
    {
        private static readonly string[] DefaultConsumerConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.EnableAutoCommit,
            ConfigurationKey.AllowAutoCreateTopics,
            ConfigurationKey.BootstrapServers,
            ConfigurationKey.BrokerVersionFallback,
            ConfigurationKey.ApiVersionFallbackMs,
            ConfigurationKey.SslCaLocation,
            ConfigurationKey.SaslUsername,
            ConfigurationKey.SaslPassword,
            ConfigurationKey.SaslMechanisms,
            ConfigurationKey.SecurityProtocol,
        };

        private static readonly string[] RequiredConsumerConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.BootstrapServers
        };

        public static ConfigurationBuilder ForConsumer => new(DefaultConsumerConfigurationKeys, RequiredConsumerConfigurationKeys);

        private static readonly string[] EmptyConfigurationKeys = new string[0];
        private static readonly NamingConvention[] DefaultNamingConventions = { NamingConvention.Default };

        private readonly string[] _configurationKeys;
        private readonly string[] _requiredConfigurationKeys;
        private readonly NamingConvention[] _namingConventions;
        private readonly ConfigurationSource _configurationSource;
        private readonly IDictionary<string, string> _configurations;
        private readonly ConfigurationReporter _configurationReporter;

        public ConfigurationBuilder()
            : this(EmptyConfigurationKeys, EmptyConfigurationKeys, DefaultNamingConventions, ConfigurationSource.Null, new Dictionary<string, string>(), ConfigurationReporter.CreateDefault())
        {
        }

        public ConfigurationBuilder(string[] configurationKeys, string[] requiredConfigurationKeys)
            : this(configurationKeys ?? EmptyConfigurationKeys, requiredConfigurationKeys ?? EmptyConfigurationKeys, DefaultNamingConventions, ConfigurationSource.Null, new Dictionary<string, string>(), ConfigurationReporter.CreateDefault())
        {
        }

        private ConfigurationBuilder(string[] configurationKeys, string[] requiredConfigurationKeys, NamingConvention[] namingConventions, ConfigurationSource configurationSource, IDictionary<string, string> configurations, ConfigurationReporter configurationReporter)
        {
            _configurationKeys = configurationKeys;
            _requiredConfigurationKeys = requiredConfigurationKeys;
            _namingConventions = namingConventions.Length == 0 ? DefaultNamingConventions : namingConventions;
            _configurationSource = configurationSource;
            _configurations = configurations;
            _configurationReporter = configurationReporter;
        }
        
        public ConfigurationBuilder WithConfigurationKeys(params string[] configurationKeys)
        {
            return new ConfigurationBuilder(configurationKeys, _requiredConfigurationKeys, _namingConventions, _configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithRequiredConfigurationKeys(params string[] requiredConfigurationKeys)
        {
            return new ConfigurationBuilder(_configurationKeys, requiredConfigurationKeys, _namingConventions, _configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithNamingConventions(params NamingConvention[] namingConventions)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, namingConventions, _configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, _namingConventions, configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurations(IDictionary<string, string> configurations)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, _namingConventions, _configurationSource, configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurationReporter(ConfigurationReporter configurationReporter)
        {
            return new ConfigurationBuilder(_configurationKeys, _requiredConfigurationKeys, _namingConventions, _configurationSource, _configurations, configurationReporter);
        }

        public Configuration Build()
        {
            var configurations = FillConfiguration();

            ValidateConfiguration(configurations);

            return new Configuration(configurations);
        }

        private IDictionary<string, string> FillConfiguration()
        {
            var configurations = new Dictionary<string, string>();

            foreach (var configuration in _configurations)
            {
                configurations[configuration.Key] = configuration.Value;
                _configurationReporter.AddManual(configuration.Key, configuration.Value);
            }

            foreach (var key in AllKeys)
            {
                if (configurations.ContainsKey(key))
                {
                    continue;
                }

                var value = GetByKey(key);
                if (value != null)
                {
                    configurations[key] = value;
                }
            }

            return configurations;
        }

        private IEnumerable<string> AllKeys => _configurationKeys.Concat(_requiredConfigurationKeys).Distinct();

        private string GetByKey(string key)
        {
            foreach (var namingConvention in _namingConventions)
            {
                var attemptedKey = namingConvention.GetKey(key);
                var value = _configurationSource.GetByKey(attemptedKey);
                if (value != null)
                {
                    _configurationReporter.AddValue(key, GetSourceName(), value, attemptedKey);
                    return value;
                }
            }

            _configurationReporter.AddMissing(key, GetSourceName(), GetAttemptedKeys(key));

            return null;
        }

        private string GetSourceName()
        {
            return _configurationSource.GetType().Name;
        }

        private string[] GetAttemptedKeys(string key)
        {
            return _namingConventions.Select(convention => convention.GetKey(key)).ToArray();
        }

        private void ValidateConfiguration(IDictionary<string, string> configurations)
        {
            foreach (var key in _requiredConfigurationKeys)
            {
                if (!configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = "Invalid configuration:" + Environment.NewLine + _configurationReporter.Report();
                    throw new InvalidConfigurationException(message);
                }
            }
        }
    }

    internal class Configuration : ReadOnlyDictionary<string, string>
    {
        public Configuration(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public string GroupId => this[ConfigurationKey.GroupId];

        public bool EnableAutoCommit
        {
            get
            {
                const bool defaultAutoCommitStrategy = true;

                if (!TryGetValue(ConfigurationKey.EnableAutoCommit, out var value))
                {
                    return defaultAutoCommitStrategy;
                }

                return bool.Parse(value);
            }
        }
    }
}