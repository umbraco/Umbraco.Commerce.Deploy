using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class RegionArtifact(
        GuidUdi? udi,
        GuidUdi storeUdi,
        GuidUdi countryUdi,
        IEnumerable<ArtifactDependency> dependencies = null)
        : StoreEntityArtifactBase(udi, storeUdi, dependencies)
    {
        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public GuidUdi CountryUdi { get; set; } = countryUdi;
        public GuidUdi? DefaultPaymentMethodUdi { get; set; }
        public GuidUdi? DefaultShippingMethodUdi { get; set; }
        public int SortOrder { get; set; }
    }
}
