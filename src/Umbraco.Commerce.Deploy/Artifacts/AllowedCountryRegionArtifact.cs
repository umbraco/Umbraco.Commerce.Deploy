using Umbraco.Cms.Core;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class AllowedCountryRegionArtifact : AllowedCountryArtifact
    {
        public GuidUdi RegionUdi { get; set; }
    }
}
