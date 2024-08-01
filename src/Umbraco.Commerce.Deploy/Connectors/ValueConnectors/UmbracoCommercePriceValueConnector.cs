using System;
using System.Collections.Generic;
using System.Text.Json;
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
    public class UmbracoCommercePriceValueConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        : ValueConnectorBase
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.Price" };

        public override async Task<string?> ToArtifactAsync(
            object? value,
            IPropertyType propertyType,
            ICollection<ArtifactDependency> dependencies,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                return null;
            }

            Dictionary<Guid, decimal?>? srcDict = JsonSerializer.Deserialize<Dictionary<Guid, decimal?>>(svalue, Constants.DefaultJsonSerializerOptions);

            var dstDict = new Dictionary<GuidUdi, decimal?>();

            foreach (KeyValuePair<Guid, decimal?> kvp in srcDict)
            {
                var udi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, kvp.Key);

                // Because we store Guid IDs anyway we don't neceserily need to fetch
                // the Currency entity to look anything up, it's mostly a question
                // of whether we want to validate the Currency exists. I'm not sure
                // whether this should really be the responsibility of the property editor
                // though and we should just be able to trust the property editor value
                // is valid?

                dependencies.Add(new UmbracoCommerceArtifactDependency(udi, ArtifactDependencyMode.Exist));

                dstDict.Add(udi, kvp.Value);
            }

            return JsonSerializer.Serialize(dstDict, Constants.DefaultJsonSerializerOptions);
        }



        public override async Task<object?> FromArtifactAsync(
            string? value,
            IPropertyType propertyType,
            object? currentValue,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                return null;
            }

            Dictionary<string, decimal?>? srcDict = JsonSerializer.Deserialize<Dictionary<string, decimal?>>(svalue, Constants.DefaultJsonSerializerOptions);

            var dstDict = new Dictionary<Guid, decimal?>();

            foreach (KeyValuePair<string, decimal?> kvp in srcDict)
            {
                if (UdiHelper.TryParseGuidUdi(kvp.Key, out GuidUdi udi) && udi.EntityType == UmbracoCommerceConstants.UdiEntityType.Currency)
                {
                    CurrencyReadOnly? currencyEntity = umbracoCommerceApi.GetCurrency(udi.Guid);
                    if (currencyEntity != null)
                    {
                        dstDict.Add(currencyEntity.Id, kvp.Value);
                    }
                }
            }

            return JsonSerializer.Serialize(dstDict, Constants.DefaultJsonSerializerOptions);
        }
    }
}
