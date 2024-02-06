using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class ShippingMethodArtifact : StoreEntityArtifactBase
    {
        public ShippingMethodArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Sku { get; set; }
        public GuidUdi TaxClassUdi { get; set; }

        [Obsolete("Now handled via CalculationConfig")]
        public IEnumerable<ServicePriceArtifact> Prices { get; set; }
        public string ImageId { get; set; }

        public int CalculationMode { get; set; }
        public JObject CalculationConfig { get; set; }
        public string ShippingProviderAlias { get; set; }
        public SortedDictionary<string, string> ShippingProviderSettings { get; set; }

        public IEnumerable<AllowedCountryRegionArtifact> AllowedCountryRegions { get; set; }
        public int SortOrder { get; set; }
    }
}
