using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Cms;
using Umbraco.Commerce.Core.Data;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Core.Configuration;
using Umbraco.Deploy.Core.Configuration.DeployConfiguration;
using Microsoft.Extensions.Configuration;

namespace Umbraco.Commerce.Deploy.Composing
{
    [ComposeAfter(typeof(UmbracoCommerceComposer))]
    public class UmbracoCommerceDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<UmbracoCommerceDeploySettings>()
                .Bind(builder.Config.GetSection("Umbraco:Commerce:Deploy"));

            // Ensure SQLite support for local development
            builder.WithUmbracoCommerceBuilder().AddSQLite();

            builder.Services.AddUnique<IConfigureOptions<ConnectionStringConfig>, ConfigureUmbracoCommerceDeployConnectionString>();

            builder.Services.AddSingleton<UmbracoCommerceDeploySettingsAccessor>();

            builder.Components()
                .Append<UmbracoCommerceDeployComponent>();

            // Deploy makes its own schema entities transfer entity types in the BackOfficeOnly edition, so do the same for the settings entities.
            // The edition is read from configuration (like Deploy does), because getting the options would validate all Deploy settings.
            if (builder.Config.GetSection(DeployConstants.ConfigurationSectionKeys.Settings).GetValue<Edition>(nameof(DeploySettings.Edition)) is Edition.BackOfficeOnly)
            {
                builder.Components().Append<UmbracoCommerceDeployBackOfficeOnlyComponent>();
            }
        }
    }
}
