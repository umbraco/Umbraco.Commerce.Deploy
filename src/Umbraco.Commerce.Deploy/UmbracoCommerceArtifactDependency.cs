using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy
{
    public class UmbracoCommerceArtifactDependency : ArtifactDependency
    {
        public UmbracoCommerceArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)
            : base(udi, false, mode)
        { }
    }
}
