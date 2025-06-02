using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Cms.Data;
using Umbraco.Commerce.Core.Data;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Commerce.Deploy.Configuration;

public class ConfigureUmbracoCommerceDeployConnectionString(IConfiguration configuration) : UmbracoConnectionStringConfigurator(configuration)
{
    public override void Configure(ConnectionStringConfig options)
        => Configure(Options.DefaultName, options);

    public override void Configure(string? name, ConnectionStringConfig options)
    {
        // Perform the default configuration
        base.Configure(name, options);

        // Only configure if we are using the default Umbraco connection string
        if (options.Name != UmbConstants.System.UmbracoConnectionName)
        {
            return;
        }

        // If Umbraco is configured to use SQLIte, then configure Umbraco Commerce to use it's own SQLite database
        if (options.ProviderName.Contains("SQLite", StringComparison.InvariantCultureIgnoreCase))
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            var ucDbConnectionString = $"Data Source={dataDirectory}/Umbraco.Commerce.sqlite.db;Mode=ReadWrite;Foreign Keys=True;Pooling=True;Cache=Private";
            var ucDbProviderName = Persistence.Sqlite.Constants.ProviderName;

            // Update configuration
            configuration[$"ConnectionStrings:{name}"] = ucDbConnectionString;
            configuration[$"ConnectionStrings:{name}{ConnectionStringConfig.ProviderNamePostfix}"] = ucDbProviderName;

            // Update options
            options.Name = name;
            options.ConnectionString = ucDbConnectionString;
            options.ProviderName = ucDbProviderName;
        }
    }
}
