using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Core.Data;
using Umbraco.Deploy.Core.Configuration.DeployConfiguration;
using Umbraco.Extensions;
using UcConstants = Umbraco.Commerce.Core.Constants;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Commerce.Deploy.Configuration;

public class ConfigureUmbracoCommerceConnectionString(IConfiguration configuration, IOptionsMonitor<DeploySettings> deploySettings) : IConfigureNamedOptions<ConnectionStringConfig>
{
    public void Configure(ConnectionStringConfig connectionStringConfig)
        => Configure(Options.DefaultName, connectionStringConfig);

    public void Configure(string? name, ConnectionStringConfig connectionStringConfig)
    {
        // Default to using Umbraco Commerce connection name
        if (name == Options.DefaultName)
        {
            name = UcConstants.System.ConnectionStringName;
        }

        // Skip non-empty connection string and only configure Umbraco Commerce connection name
        if ((connectionStringConfig != null
            && !string.IsNullOrWhiteSpace(connectionStringConfig.ConnectionString)
            && !string.IsNullOrWhiteSpace(connectionStringConfig.ProviderName))
            || name != UcConstants.System.ConnectionStringName)
        {
            return;
        }

        // Get Umbraco connection string
        _ = configuration.GetUmbracoConnectionString(UmbConstants.System.UmbracoConnectionName, out var providerName);
        var umbDbProviderName = providerName;

        // If Umbraco is configured to use SQLIte, then configure Umbraco Commerce to use it's own SQLite database
        if (umbDbProviderName != null && umbDbProviderName.Contains("SQLite", StringComparison.InvariantCultureIgnoreCase))
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            var ucDbConnectionString = $"Data Source={dataDirectory}/Umbraco.Commerce.sqlite.db;Mode=ReadWrite;Foreign Keys=True;Pooling=True;Cache=Private";
            var ucDbProviderName = Persistence.Sqlite.Constants.ProviderName;

            // Update configuration
            configuration[$"ConnectionStrings:{name}"] = ucDbConnectionString;
            configuration[$"ConnectionStrings:{name}{ConnectionStringConfig.ProviderNamePostfix}"] = ucDbProviderName;

            // Update options
            connectionStringConfig.Name = name;
            connectionStringConfig.ConnectionString = ucDbConnectionString;
            connectionStringConfig.ProviderName = ucDbProviderName;
        }
    }
}
