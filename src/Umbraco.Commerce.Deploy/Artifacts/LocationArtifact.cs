using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Deploy.Artifacts
{
    public class LocationArtifact : StoreEntityArtifactBase
    {
        public LocationArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string CountryIsoCode { get; set; }
        public string ZipCode { get; set; }
        public LocationType Type { get; set; }
        public int SortOrder { get; set; }
    }
}
