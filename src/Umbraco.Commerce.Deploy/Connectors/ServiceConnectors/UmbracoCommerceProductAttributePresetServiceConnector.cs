using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi)]
    public class UmbracoCommerceProductAttributePresetServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ProductAttributePresetArtifact, ProductAttributePresetReadOnly, ProductAttributePreset, ProductAttributePresetState>
    {
        protected override int[] ProcessPasses => new[]
        {
            2,4
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All global:: Product Attribute Presets";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset;

        public override string ContainerId => Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributePresets].ToInvariantString();

        public UmbracoCommerceProductAttributePresetServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ProductAttributePresetReadOnly entity)
            => entity.Name;

        public override Task<ProductAttributePresetReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((ProductAttributePresetReadOnly?)_umbracoCommerceApi.GetProductAttributePreset(id));

        public override IAsyncEnumerable<ProductAttributePresetReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetProductAttributePresets(storeId).ToAsyncEnumerable();

        public override Task<ProductAttributePresetArtifact?> GetArtifactAsync(GuidUdi? udi, ProductAttributePresetReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<ProductAttributePresetArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var allowedAttributes = new List<AllowedProductAttributeArtifact>();

            foreach (AllowedProductAttribute? allowedAttr in entity.AllowedAttributes)
            {
                // Get product attribute ID
                ProductAttributeReadOnly? attr = _umbracoCommerceApi.GetProductAttribute(entity.StoreId, allowedAttr.ProductAttributeAlias);
                var attrUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, attr.Id);

                // Add the product attribute as a dependency
                dependencies.Add(new UmbracoCommerceArtifactDependency(attrUdi));

                // Add the allowed attribute to the collection of attributes
                allowedAttributes.Add(new AllowedProductAttributeArtifact
                {
                    ProductAttributeUdi = attrUdi,
                    AllowedValueAliases = allowedAttr.AllowedValueAliases
                });
            }

            var artifact = new ProductAttributePresetArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Alias,
                Icon = entity.Icon,
                Description = entity.Description,
                AllowedAttributes = allowedAttributes,
                SortOrder = entity.SortOrder
            };

            return Task.FromResult<ProductAttributePresetArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 2:
                    await Pass2Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                case 4:
                    await Pass4Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private Task Pass2Async(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    ProductAttributePresetArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    ProductAttributePreset? entity = state.Entity?.AsWritable(uow) ?? ProductAttributePreset.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetAlias(artifact.Alias)
                        .SetName(artifact.Name)
                        .SetIcon(artifact.Icon)
                        .SetDescription(artifact.Description)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveProductAttributePreset(entity);

                    state.Entity = entity;

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);

        private Task Pass4Async(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    ProductAttributePresetArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        ProductAttributePreset? entity = _umbracoCommerceApi.GetProductAttributePreset(state.Entity.Id).AsWritable(uow);

                        var productAttributeAliasMap = _umbracoCommerceApi.GetProductAttributes(artifact.AllowedAttributes.Select(x => x.ProductAttributeUdi.Guid).ToArray())
                            .ToDictionary(x => x.Id, x => x);

                        var allowedAttributes = new List<AllowedProductAttribute>();

                        foreach (AllowedProductAttributeArtifact allowedAttr in artifact.AllowedAttributes.Where(x => productAttributeAliasMap.ContainsKey(x.ProductAttributeUdi.Guid)))
                        {
                            ProductAttributeReadOnly? attr = productAttributeAliasMap[allowedAttr.ProductAttributeUdi.Guid];

                            allowedAttributes.Add(new AllowedProductAttribute(
                                attr.Alias,
                                allowedAttr.AllowedValueAliases
                                    .Where(x => attr.Values.Any(y => y.Alias.InvariantEquals(x)))
                                    .ToList()));
                        }

                        entity.SetAllowedAttributes(allowedAttributes, SetBehavior.Replace);

                        _umbracoCommerceApi.SaveProductAttributePreset(entity);
                    }

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
