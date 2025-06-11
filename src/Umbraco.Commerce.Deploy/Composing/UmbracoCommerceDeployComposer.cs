using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Cms;
using Umbraco.Commerce.Core.Data;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

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
        }
    }
}
