using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Configuration;

internal class NamingConventions : IEnumerable<NamingConvention>
{
    private readonly List<NamingConvention> _namingConventions = new();

    public void Add(NamingConvention namingConvention)
    {
        _namingConventions.Add(namingConvention);
    }

    public void Add(Func<string, string> converter)
    {
        Add(NamingConvention.UseCustom(converter));
    }

    public void Add(string prefix = null, params string[] additionalPrefixes)
    {
        Add(NamingConvention.UseEnvironmentStyle(prefix));

        foreach (var additionalPrefix in additionalPrefixes)
        {
            Add(NamingConvention.UseEnvironmentStyle(additionalPrefix));
        }
    }

    public IEnumerator<NamingConvention> GetEnumerator()
    {
        return _namingConventions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public string[] GetAttemptedKeys(string key)
    {
        return _namingConventions
            .Select(convention => convention.GetKey(key))
            .ToArray();
    }
}