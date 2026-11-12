using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Models;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommercePriceValueConnector : ValueConnectorBase
    {
        private IUmbracoCommerceApi _umbracoCommerceApi;
        private IJsonSerializer _jsonSerializer;
        private ILogger<UmbracoCommercePriceValueConnector>? _logger;

        public UmbracoCommercePriceValueConnector(IUmbracoCommerceApi umbracoCommerceApi,
            IJsonSerializer jsonSerializer,
            ILogger<UmbracoCommercePriceValueConnector> logger)
        {
            _umbracoCommerceApi = umbracoCommerceApi ?? throw new ArgumentNullException(nameof(umbracoCommerceApi));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _logger = logger;
        }

        public UmbracoCommercePriceValueConnector(IUmbracoCommerceApi umbracoCommerceApi,
            IJsonSerializer jsonSerializer)
        {
            _umbracoCommerceApi = umbracoCommerceApi ?? throw new ArgumentNullException(nameof(umbracoCommerceApi));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _logger = null;
        }

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

            try
            {
                var dstDict = new Dictionary<string, decimal?>();

                foreach (KeyValuePair<string, decimal?> kvp in ParsePrices(svalue, propertyType.Alias))
                {
                    if (!Guid.TryParse(kvp.Key, out Guid currencyId))
                    {
                        continue;
                    }

                    var udi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, currencyId);

                    // Because we store Guid IDs anyway we don't necessarily need to fetch
                    // the Currency entity to look anything up, it's mostly a question
                    // of whether we want to validate the Currency exists. I'm not sure
                    // whether this should really be the responsibility of the property editor
                    // though and we should just be able to trust the property editor value
                    // is valid?

                    dependencies.Add(new UmbracoCommerceArtifactDependency(udi, ArtifactDependencyMode.Exist));

                    dstDict.Add(udi.ToString(), kvp.Value);
                }

                return _jsonSerializer.Serialize(dstDict);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Failed to serialize price value to artifact: {Value}", svalue);
                return null;
            }
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

            try
            {
                Dictionary<string, decimal?> srcDict = ParsePrices(svalue, propertyType.Alias);

                var dstDict = new Dictionary<Guid, decimal?>();

                if (srcDict != null)
                {
                    foreach (KeyValuePair<string, decimal?> kvp in srcDict)
                    {
                        if (UdiHelper.TryParseGuidUdi(kvp.Key, out GuidUdi? udi) &&
                            udi!.EntityType == UmbracoCommerceConstants.UdiEntityType.Currency)
                        {
                            CurrencyReadOnly? currencyEntity = await _umbracoCommerceApi.GetCurrencyAsync(udi.Guid);
                            if (currencyEntity != null)
                            {
                                dstDict.Add(currencyEntity.Id, kvp.Value);
                            }
                            else
                            {
                                _logger?.Warn("Could not resolve currency {CurrencyUdi} on the target environment. The price for this currency will be dropped from {PropertyTypeAlias}.", udi.ToString(), propertyType.Alias);
                            }
                        }
                    }
                }

                return _jsonSerializer.Serialize(dstDict);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Failed to deserialize price value from artifact: {Value}", svalue);
                return null;
            }
        }

        /// <summary>
        /// Reads a price value: a JSON object of currency key to amount. An amount can be a number,
        /// <c>null</c>, or a string (the editor saves <c>""</c> for a currency with no price), so an
        /// empty string is read as no price for that currency instead of failing the whole value.
        /// </summary>
        private Dictionary<string, decimal?> ParsePrices(string value, string propertyTypeAlias)
        {
            var prices = new Dictionary<string, decimal?>();

            if (JsonNode.Parse(value) is not JsonObject json)
            {
                return prices;
            }

            foreach (KeyValuePair<string, JsonNode?> kvp in json)
            {
                prices[kvp.Key] = ParseAmount(kvp.Value, kvp.Key, propertyTypeAlias);
            }

            return prices;
        }

        private decimal? ParseAmount(JsonNode? node, string currencyKey, string propertyTypeAlias)
        {
            if (node is not JsonValue jsonValue)
            {
                return null;
            }

            switch (jsonValue.GetValueKind())
            {
                case JsonValueKind.Number:
                    return jsonValue.GetValue<decimal>();
                case JsonValueKind.String:
                    var amount = jsonValue.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(amount))
                    {
                        return null;
                    }

                    if (decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                    {
                        return parsed;
                    }

                    _logger?.Warn("Could not read the price {Amount} for currency {CurrencyKey} in {PropertyTypeAlias}. It will be treated as no price.", amount, currencyKey, propertyTypeAlias);
                    return null;
                default:
                    return null;
            }
        }
    }
}
