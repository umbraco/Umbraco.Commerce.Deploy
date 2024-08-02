using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Models;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommerceStorePickerValueConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        : ValueConnectorBase
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.StorePicker" };

        public override async Task<string?> ToArtifactAsync(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                return null;
            }

            if (!Guid.TryParse(svalue, out Guid storeId))
            {
                return null;
            }

            StoreReadOnly? store = umbracoCommerceApi.GetStore(storeId);
            if (store == null)
            {
                return null;
            }

            var udi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, storeId);

            dependencies.Add(new UmbracoCommerceArtifactDependency(udi));

            return udi.ToString();
        }

        public override async Task<object?> FromArtifactAsync(
            string? value,
            IPropertyType propertyType,
            object? currentValue,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out GuidUdi? udi) || udi!.EntityType != UmbracoCommerceConstants.UdiEntityType.Store)
            {
                return null;
            }

            StoreReadOnly? store = umbracoCommerceApi.GetStore(udi.Guid);

            if (store != null)
            {
                return store.Id.ToString();
            }

            return null;
        }
    }
}
