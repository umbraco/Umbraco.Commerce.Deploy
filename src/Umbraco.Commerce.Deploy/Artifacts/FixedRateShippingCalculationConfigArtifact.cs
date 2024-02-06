using System.Collections.Generic;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class FixedRateShippingCalculationConfigArtifact
    {
        public IEnumerable<ServicePriceArtifact> Prices { get; set; }
    }
}
