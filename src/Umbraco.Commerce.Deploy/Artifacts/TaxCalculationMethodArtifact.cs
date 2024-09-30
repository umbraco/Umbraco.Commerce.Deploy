using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Serialization;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class TaxCalculationMethodArtifact(GuidUdi? udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
        : StoreEntityArtifactBase(udi, storeUdi, dependencies)
    {
        public string SalesTaxProviderAlias { get; set; }
        public SortedDictionary<string, string> SalesTaxProviderSettings { get; set; }
        public int SortOrder { get; set; }
    }
}
