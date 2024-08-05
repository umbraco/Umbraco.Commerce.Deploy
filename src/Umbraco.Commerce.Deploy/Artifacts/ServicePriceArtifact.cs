using System.Text.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Deploy.Infrastructure.Serialization;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class ServicePriceArtifact
    {
        public GuidUdi CurrencyUdi { get; set; }
        public GuidUdi? CountryUdi { get; set; }
        public GuidUdi? RegionUdi { get; set; }

        [RoundingDecimalConverter(4)]
        public decimal Value { get; set; }
    }
}
