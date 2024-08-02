using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Deploy.Infrastructure.Connectors.ValueConnectors;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Commerce.Core.Models;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the Umbraco Commerce Variants Editor property editor
    /// </summary>
    public class UmbracoCommerceVariantsEditorValueConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        IContentTypeService contentTypeService,
        Lazy<ValueConnectorCollection> valueConnectors,
        PropertyTypeMigratorCollection propertyTypeMigrators,
        IJsonSerializer jsonSerializer,
        ILogger<UmbracoCommerceVariantsEditorValueConnector> logger)
        : BlockEditorValueConnectorBase<UmbracoCommerceVariantsEditorValueConnector.VariantsBlockEditorValue>(
            jsonSerializer, contentTypeService, valueConnectors, propertyTypeMigrators, logger), IValueConnector
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.VariantsEditor" };

        public override async Task<string?> ToArtifactAsync(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            if (value is null)
            {
                return null;
            }

            if (!jsonSerializer.TryDeserialize(value, out VariantsBlockEditorValueBase? storeValue) || !storeValue.StoreId.HasValue)
            {
                return null;
            }

            if (!jsonSerializer.TryDeserialize(value, out VariantsBlockEditorValue? result))
            {
                return null;
            }

            await ToArtifactAsync(result, dependencies, contextCache, cancellationToken).ConfigureAwait(false);

            IEnumerable<string>? productAttributeAliases = result.GetLayouts()?.SelectMany(x => x.Config.Attributes.Keys)
                .Distinct();

            if (productAttributeAliases != null)
            {
                foreach (var productAttributeAlias in productAttributeAliases)
                {
                    ProductAttributeReadOnly? productAttribute = umbracoCommerceApi.GetProductAttribute(storeValue.StoreId.Value, productAttributeAlias);
                    if (productAttribute != null)
                    {
                        dependencies.Add(new UmbracoCommerceArtifactDependency(productAttribute.GetUdi()));
                    }
                }
            }

            result.StoreId = storeValue.StoreId;

            var artifact = jsonSerializer.Serialize(result);

            JsonObject? artifactJson = jsonSerializer.Deserialize<JsonObject>(artifact.ToString()!);

            artifactJson!.Remove("storeId");
            artifactJson!.Add("storeId", storeValue.StoreId);

            return jsonSerializer.Serialize(artifactJson);
        }

        public override async Task<object?> FromArtifactAsync(string? value, IPropertyType propertyType, object? currentValue,
            IDictionary<string, string>? propertyEditorAliases, IContextCache contextCache,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var artifact = await base.FromArtifactAsync(value, propertyType, currentValue, propertyEditorAliases,
                contextCache, cancellationToken).ConfigureAwait(false);

            if (artifact == null)
            {
                return null;
            }

            // If we have an artifact, value can't be null so we can just parse it for a store id
            if (!jsonSerializer.TryDeserialize(value!, out VariantsBlockEditorValueBase? storeValue) || !storeValue.StoreId.HasValue)
            {
                return artifact;
            }

            JsonObject? artifactJson = jsonSerializer.Deserialize<JsonObject>(artifact.ToString()!);

            artifactJson!.Remove("storeId");
            artifactJson!.Add("storeId", storeValue.StoreId);

            return jsonSerializer.Serialize(artifactJson);
        }

        private class VariantsBlockEditorValueBase
        {
            public Guid? StoreId { get; set; }
        }

        public class VariantsBlockEditorValue : BlockValue<VariantsBlockEditorLayoutItem>
        {
            public override string PropertyEditorAlias => "Umbraco.Commerce.VariantsEditor";

            public Guid? StoreId { get; set; }
        }

        public class VariantsBlockEditorLayoutItem : IBlockLayoutItem
        {
            public Udi? ContentUdi { get; set; }

            public Udi? SettingsUdi { get; set; }
            public ProductVariantConfig Config { get; set; }
        }

        public class ProductVariantConfig
        {
            public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

            public bool IsDefault { get; set; }
        }
    }
}
