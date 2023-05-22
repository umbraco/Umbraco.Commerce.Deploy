using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Commerce.Deploy.Composing
{
    public class UmbracoCommerceDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<UmbracoCommerceDeploySettings>()
                .Bind(builder.Config.GetSection("Umbraco:Commerce:Deploy"));

            builder.Services.AddSingleton<UmbracoCommerceDeploySettingsAccessor>();

            builder.Components()
                .Append<UmbracoCommerceDeployComponent>();
        }
    }
}
