using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Umbraco.Commerce.Deploy.Configuration
{
    public class UmbracoCommerceDeploySettingsAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        public UmbracoCommerceDeploySettingsAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public UmbracoCommerceDeploySettings Settings => _serviceProvider.GetRequiredService<IOptions<UmbracoCommerceDeploySettings>>().Value;
    }
}
