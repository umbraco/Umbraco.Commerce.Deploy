using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Commerce.Deploy
{
    public class UmbracoCommerceDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            if (!builder.ManifestFilters().Has<UmbracoCommerceDeployManifestFilter>())
            {
                builder.ManifestFilters().Append<UmbracoCommerceDeployManifestFilter>();
            }
        }
    }
}
