using System;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration
{
    internal record ConfigurationBuilder(ConfigurationKeys ConfigurationKeys)
    {
        private static readonly NamingConvention[] DefaultNamingConventions = { NamingConvention.Default };
        private static readonly IDictionary<string, string> Empty = new Dictionary<string, string>();

        public static ConfigurationBuilder ForConsumer => new ConfigurationBuilder(ConfigurationKeys.Consumer);

        private ConfigurationKeys ConfigurationKeys { get; } = ConfigurationKeys;
        private NamingConvention[] NamingConventions { get; init; } = DefaultNamingConventions;
        private ConfigurationSource ConfigurationSource { get; init; } = ConfigurationSource.Null;
        private IDictionary<string, string> Configurations { get; init; } = Empty;
        private ConfigurationReporter ConfigurationReporter { get; init; } = ConfigurationReporter.CreateDefault();

        public ConfigurationBuilder WithNamingConventions(params NamingConvention[] namingConventions)
        {
            return this with { NamingConventions = namingConventions };
        }

        public ConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            return this with { ConfigurationSource = configurationSource };
        }

        public ConfigurationBuilder WithConfigurations(IDictionary<string, string> configurations)
        {
            return this with { Configurations = new Dictionary<string, string>(configurations) };
        }

        public ConfigurationBuilder WithConfigurationReporter(ConfigurationReporter configurationReporter)
        {
            return this with { ConfigurationReporter = configurationReporter };
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

            foreach (var configuration in Configurations)
            {
                configurations[configuration.Key] = configuration.Value;
                ConfigurationReporter.AddManual(configuration.Key, configuration.Value);
            }

            foreach (var key in ConfigurationKeys)
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
            foreach (var namingConvention in NamingConventions)
            {
                var attemptedKey = namingConvention.GetKey(key);
                var value = ConfigurationSource.GetByKey(attemptedKey);
                if (value != null)
                {
                    ConfigurationReporter.AddValue(key, GetSourceName(), value, attemptedKey);
                    return value;
                }
            }

            ConfigurationReporter.AddMissing(key, GetSourceName(), GetAttemptedKeys(key));

            return null;
        }

        private string GetSourceName()
        {
            return ConfigurationSource.GetType().Name;
        }

        private string[] GetAttemptedKeys(string key)
        {
            return NamingConventions.Select(convention => convention.GetKey(key)).ToArray();
        }

        private void ValidateConfiguration(IDictionary<string, string> configurations)
        {
            foreach (var key in ConfigurationKeys.Required)
            {
                if (!configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = "Invalid configuration:" + Environment.NewLine + ConfigurationReporter.Report();
                    throw new InvalidConfigurationException(message);
                }
            }
        }
    }
}