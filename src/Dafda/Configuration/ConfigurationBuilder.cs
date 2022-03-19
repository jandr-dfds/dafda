using System;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration
{
    internal class ConfigurationBuilder
    {
        private static readonly NamingConvention[] DefaultNamingConventions = { NamingConvention.Default };

        public static ConfigurationBuilder ForConsumer => new ConfigurationBuilder(ConfigurationKeys.Consumer);

        private readonly ConfigurationKeys _configurationKeys;
        private readonly NamingConvention[] _namingConventions;
        private readonly ConfigurationSource _configurationSource;
        private readonly IDictionary<string, string> _configurations;
        private readonly ConfigurationReporter _configurationReporter;

        public ConfigurationBuilder(ConfigurationKeys configurationKeys)
            : this(configurationKeys, DefaultNamingConventions, ConfigurationSource.Null, new Dictionary<string, string>(), ConfigurationReporter.CreateDefault())
        {
        }

        private ConfigurationBuilder(ConfigurationKeys configurationKeys, NamingConvention[] namingConventions, ConfigurationSource configurationSource, IDictionary<string, string> configurations, ConfigurationReporter configurationReporter)
        {
            _configurationKeys = configurationKeys;
            _namingConventions = namingConventions.Length == 0 ? DefaultNamingConventions : namingConventions;
            _configurationSource = configurationSource;
            _configurations = configurations;
            _configurationReporter = configurationReporter;
        }

        public ConfigurationBuilder WithNamingConventions(params NamingConvention[] namingConventions)
        {
            return new ConfigurationBuilder(_configurationKeys, namingConventions, _configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            return new ConfigurationBuilder(_configurationKeys, _namingConventions, configurationSource, _configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurations(IDictionary<string, string> configurations)
        {
            return new ConfigurationBuilder(_configurationKeys, _namingConventions, _configurationSource, configurations, _configurationReporter);
        }

        public ConfigurationBuilder WithConfigurationReporter(ConfigurationReporter configurationReporter)
        {
            return new ConfigurationBuilder(_configurationKeys, _namingConventions, _configurationSource, _configurations, configurationReporter);
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

            foreach (var key in _configurationKeys)
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
            foreach (var key in _configurationKeys.Required)
            {
                if (!configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = "Invalid configuration:" + Environment.NewLine + _configurationReporter.Report();
                    throw new InvalidConfigurationException(message);
                }
            }
        }
    }
}