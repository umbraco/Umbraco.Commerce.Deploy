using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using System.Linq;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi)]
    public class UmbracoCommerceProductAttributePresetServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ProductAttributePresetArtifact, ProductAttributePresetReadOnly, ProductAttributePreset, ProductAttributePresetState>
    {
        public override int[] ProcessPasses => new[]
        {
            2,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All global:: Product Attribute Presets";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset;

        public override string ContainerId => Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributePresets].ToInvariantString();

        public UmbracoCommerceProductAttributePresetServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ProductAttributePresetReadOnly entity)
            => entity.Name;

        public override ProductAttributePresetReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetProductAttributePreset(id);

        public override IEnumerable<ProductAttributePresetReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetProductAttributePresets(storeId);

        public override ProductAttributePresetArtifact GetArtifact(GuidUdi udi, ProductAttributePresetReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var allowedAttributes = new List<AllowedProductAttributeArtifact>();

            foreach (var allowedAttr in entity.AllowedAttributes)
            {
                // Get product attribute ID
                var attr = _umbracoCommerceApi.GetProductAttribute(entity.StoreId, allowedAttr.ProductAttributeAlias);
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

            return artifact;
        }

        public override void Process(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 2:
                    Pass2(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass2(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ProductAttributePreset.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetAlias(artifact.Alias)
                    .SetName(artifact.Name)
                    .SetIcon(artifact.Icon)
                    .SetDescription(artifact.Description)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveProductAttributePreset(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetProductAttributePreset(state.Entity.Id).AsWritable(uow);

                var productAttributeAliasMap = _umbracoCommerceApi.GetProductAttributes(artifact.AllowedAttributes.Select(x => x.ProductAttributeUdi.Guid).ToArray())
                    .ToDictionary(x => x.Id, x => x);

                var allowedAttributes = new List<AllowedProductAttribute>();

                foreach (var allowedAttr in artifact.AllowedAttributes.Where(x => productAttributeAliasMap.ContainsKey(x.ProductAttributeUdi.Guid)))
                {
                    var attr = productAttributeAliasMap[allowedAttr.ProductAttributeUdi.Guid];

                    allowedAttributes.Add(new AllowedProductAttribute(attr.Alias, 
                        allowedAttr.AllowedValueAliases
                            .Where(x => attr.Values.Any(y => y.Alias.InvariantEquals(x)))
                            .ToList()));
                }

                entity.SetAllowedAttributes(allowedAttributes, SetBehavior.Replace);

                _umbracoCommerceApi.SaveProductAttributePreset(entity);

                uow.Complete();
            });
        }
    }
}
