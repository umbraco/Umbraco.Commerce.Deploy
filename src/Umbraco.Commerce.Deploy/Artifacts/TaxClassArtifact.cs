using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Serialization;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class TaxClassArtifact(GuidUdi? udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
        : StoreEntityArtifactBase(udi, storeUdi, dependencies)
    {
        [RoundingDecimalConverter(3)]
        public decimal DefaultTaxRate { get; set; }

        public string DefaultTaxCode { get; set; }

        public IEnumerable<CountryRegionTaxClassArtifact>? CountryRegionTaxClasses { get; set; }

        public int SortOrder { get; set; }
    }

    public class CountryRegionTaxClassArtifact
    {
        public GuidUdi CountryUdi { get; set; }

        public GuidUdi? RegionUdi { get; set; }

        [RoundingDecimalConverter(3)]
        public decimal TaxRate { get; set; }

        public string TaxCode { get; set; }
    }
}
