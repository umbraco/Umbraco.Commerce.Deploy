using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Umbraco.Commerce.Deploy.Configuration
{
    public class UmbracoCommerceDeploySettingsAccessor(IServiceProvider serviceProvider)
    {
        public UmbracoCommerceDeploySettings Settings => serviceProvider.GetRequiredService<IOptions<UmbracoCommerceDeploySettings>>().Value;
    }
}
