using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class CountryArtifact(GuidUdi? udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
        : StoreEntityArtifactBase(udi, storeUdi, dependencies)
    {
        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public GuidUdi? DefaultCurrencyUdi { get; set; }
        public GuidUdi? DefaultPaymentMethodUdi { get; set; }
        public GuidUdi? DefaultShippingMethodUdi { get; set; }
        public int SortOrder { get; set; }
    }
}
