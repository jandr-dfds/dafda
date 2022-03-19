using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dafda.Configuration
{
    internal class Configuration : ReadOnlyDictionary<string, string>
    {
        public Configuration(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public string GroupId => this[ConfigurationKeys.GroupId];

        public bool EnableAutoCommit
        {
            get
            {
                const bool defaultAutoCommitStrategy = true;

                if (!TryGetValue(ConfigurationKeys.EnableAutoCommit, out var value))
                {
                    return defaultAutoCommitStrategy;
                }

                return bool.Parse(value);
            }
        }
    }
}