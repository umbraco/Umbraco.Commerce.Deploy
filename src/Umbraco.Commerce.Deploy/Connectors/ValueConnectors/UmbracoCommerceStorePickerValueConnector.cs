using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommerceStorePickerValueConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor,
        ILogger<UmbracoCommerceStorePickerValueConnector> logger)
        : ValueConnectorBase
    {
        [Obsolete("Use the constructor that accepts an ILogger instead. Will be removed in v19.0.0")]
        public UmbracoCommerceStorePickerValueConnector(
            IUmbracoCommerceApi umbracoCommerceApi,
            UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : this(umbracoCommerceApi, settingsAccessor, StaticServiceProvider.Instance.GetRequiredService<ILogger<UmbracoCommerceStorePickerValueConnector>>())
        { }

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

            StoreReadOnly? store = await umbracoCommerceApi.GetStoreAsync(storeId);

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

            StoreReadOnly? store = await umbracoCommerceApi.GetStoreAsync(udi.Guid);

            if (store == null)
            {
                logger.Warn("Could not resolve store {StoreUdi} on the target environment. The value for {PropertyTypeAlias} will be dropped.", udi.ToString(), propertyType.Alias);
                return null;
            }

            return store.Id.ToString();
        }
    }
}
