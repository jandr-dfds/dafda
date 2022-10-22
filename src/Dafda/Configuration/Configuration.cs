using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dafda.Configuration
{
    internal class Configuration : ReadOnlyDictionary<string, string>
    {
        public Configuration(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }
    }
}