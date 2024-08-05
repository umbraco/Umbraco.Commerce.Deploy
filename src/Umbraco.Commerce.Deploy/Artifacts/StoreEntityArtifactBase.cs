using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public abstract class StoreEntityArtifactBase(
        GuidUdi? udi,
        GuidUdi storeUdi,
        IEnumerable<ArtifactDependency>? dependencies = null)
        : DeployArtifactBase<GuidUdi>(udi, dependencies)
    {
        public GuidUdi StoreUdi { get; set; } = storeUdi;
    }
}
