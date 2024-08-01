using System.Text.Json.Serialization;
using Umbraco.Cms.Core;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class ServicePriceArtifact
    {
        public GuidUdi CurrencyUdi { get; set; }
        public GuidUdi? CountryUdi { get; set; }
        public GuidUdi? RegionUdi { get; set; }

        // [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal Value { get; set; }
    }
}
