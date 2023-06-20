using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Commerce.Deploy
{
    public class UmbracoCommerceDeployManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            var manifest = new PackageManifest()
            {
                PackageName = "Umbraco Commerce Deploy",
                Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion?.Split('+')[0],
                BundleOptions = BundleOptions.None,
                AllowPackageTelemetry = true
            };

            manifests.Add(manifest);
        }
    }
}
