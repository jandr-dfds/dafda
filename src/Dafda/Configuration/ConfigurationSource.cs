using Microsoft.Extensions.Configuration;

namespace Dafda.Configuration;

/// <summary>
/// The configuration source base 
/// </summary>
public abstract class ConfigurationSource
{
    internal static readonly ConfigurationSource Null = new NullConfigurationSource();

    /// <summary>
    /// Get the value of a configuration item by <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <returns>The configuration value</returns>
    public abstract string GetByKey(string key);

    #region Null Object

    private class NullConfigurationSource : ConfigurationSource
    {
        public override string GetByKey(string key)
        {
            return null;
        }
    }

    #endregion
}

internal class DefaultConfigurationSource : ConfigurationSource
{
    private readonly IConfiguration _configuration;

    public DefaultConfigurationSource(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override string GetByKey(string key)
    {
        return _configuration[key];
    }
}