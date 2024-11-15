using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi)]
    public class UmbracoCommerceProductAttributeServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ProductAttributeArtifact, ProductAttributeReadOnly, ProductAttribute, ProductAttributeState>
    {
        protected override int[] ProcessPasses => new[]
        {
            3
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All Umbraco Commerce Product Attributes";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ProductAttribute;

        public override string ContainerId => Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributes].ToInvariantString();

        public UmbracoCommerceProductAttributeServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ProductAttributeReadOnly entity)
            => entity.Name;

        public override Task<ProductAttributeReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetProductAttributeAsync(id);

        public override IAsyncEnumerable<ProductAttributeReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetProductAttributesAsync(storeId).AsAsyncEnumerable();

        public override Task<ProductAttributeArtifact?> GetArtifactAsync(GuidUdi? udi, ProductAttributeReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<ProductAttributeArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new ProductAttributeArtifact(udi, storeUdi, dependencies)
            {
                Name = new TranslatedValueArtifact<string>
                {
                    Translations = new SortedDictionary<string, string>(entity.Name.GetTranslatedValues().ToDictionary(x => x.Key, x => x.Value)),
                    DefaultValue = entity.Name.GetDefaultValue()
                },
                Code = entity.Alias,
                Values = entity.Values.Select(x => new ProductAttributeValueArtifact
                {
                    Alias = x.Alias,
                    Name = new TranslatedValueArtifact<string>
                    {
                        Translations = new SortedDictionary<string, string>(x.Name.GetTranslatedValues().ToDictionary(y => y.Key, y => y.Value)),
                        DefaultValue = x.Name.GetDefaultValue()
                    }
                }).ToList(),
                SortOrder = entity.SortOrder
            };

            return Task.FromResult<ProductAttributeArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 3:
                    await Pass3Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private async Task Pass3Async(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    ProductAttributeArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ProductAttribute);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    ProductAttribute? entity = state.Entity != null ? await state.Entity.AsWritableAsync(uow) : await ProductAttribute.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name.DefaultValue);

                    await entity.SetAliasAsync(artifact.Alias)
                        .SetNameAsync(new TranslatedValue<string>(artifact.Name.DefaultValue, artifact.Name.Translations))
                        .SetValuesAsync(artifact.Values.Select(x => new KeyValuePair<string, TranslatedValue<string>>(x.Alias, new TranslatedValue<string>(x.Name.DefaultValue, x.Name.Translations))))
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveProductAttributeAsync(entity, ct);

                    state.Entity = entity;

                    uow.Complete();
                },
                cancellationToken);
    }
}
