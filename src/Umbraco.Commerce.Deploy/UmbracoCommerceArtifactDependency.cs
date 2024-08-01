using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy
{
    public class UmbracoCommerceArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)
        : ArtifactDependency(udi, false, mode);
}
