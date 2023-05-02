using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class OrderStatusArtifact : StoreEntityArtifactBase
    {
        public OrderStatusArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Color { get; set; }

        public int SortOrder { get; set; }
    }
}
