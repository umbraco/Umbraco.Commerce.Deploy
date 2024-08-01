using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class TaxClassArtifact(GuidUdi? udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
        : StoreEntityArtifactBase(udi, storeUdi, dependencies)
    {
        // [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal DefaultTaxRate { get; set; }

        public IEnumerable<CountryRegionTaxRateArtifact>? CountryRegionTaxRates { get; set; }

        public int SortOrder { get; set; }
    }

    public class CountryRegionTaxRateArtifact
    {
        public GuidUdi CountryUdi { get; set; }

        public GuidUdi? RegionUdi { get; set; }

        // [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal TaxRate { get; set; }
    }
}
