using System;
using System.Collections.Generic;

namespace Dafda.Configuration
{
    internal record ConfigurationBuilder(ConfigurationKeys ConfigurationKeys)
    {
        private static readonly NamingConventions DefaultNamingConventions = new() { NamingConvention.Default };
        private static readonly IDictionary<string, string> Empty = new Dictionary<string, string>();

        public static ConfigurationBuilder ForConsumer => new ConfigurationBuilder(ConfigurationKeys.Consumer);
        public static ConfigurationBuilder ForProducer => new ConfigurationBuilder(ConfigurationKeys.Producer);

        private ConfigurationKeys ConfigurationKeys { get; } = ConfigurationKeys;
        private NamingConventions NamingConventions { get; init; } = DefaultNamingConventions;
        private ConfigurationSource ConfigurationSource { get; init; } = ConfigurationSource.Null;
        private IDictionary<string, string> Configurations { get; init; } = Empty;
        private ConfigurationReporter ConfigurationReporter { get; init; } = ConfigurationReporter.CreateDefault();

        public ConfigurationBuilder WithNamingConventions(NamingConventions namingConventions)
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

            ConfigurationReporter.AddMissing(key, GetSourceName(), NamingConventions.GetAttemptedKeys(key));

            return null;
        }

        private string GetSourceName()
        {
            return ConfigurationSource.GetType().Name;
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