using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi)]
    public class UmbracoCommerceProductAttributeServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ProductAttributeArtifact, ProductAttributeReadOnly, ProductAttribute, ProductAttributeState>
    {
        public override int[] ProcessPasses => new[]
        {
            3
        };

        public override string[] ValidOpenSelectors => new[]
        { 
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Umbraco Commerce Product Attributes";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ProductAttribute;

        public override string ContainerId => Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributes].ToInvariantString();

        public UmbracoCommerceProductAttributeServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ProductAttributeReadOnly entity)
            => entity.Name;

        public override ProductAttributeReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetProductAttribute(id);

        public override IEnumerable<ProductAttributeReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetProductAttributes(storeId);

        public override ProductAttributeArtifact GetArtifact(GuidUdi udi, ProductAttributeReadOnly entity)
        {
            if (entity == null)
                return null;

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

            return artifact;
        }

        public override void Process(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 3:
                    Pass3(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass3(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ProductAttribute);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ProductAttribute.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name.DefaultValue);

                entity.SetAlias(artifact.Alias)
                    .SetName(new TranslatedValue<string>(artifact.Name.DefaultValue, artifact.Name.Translations))
                    .SetValues(artifact.Values.Select(x => new KeyValuePair<string, TranslatedValue<string>>(x.Alias, new TranslatedValue<string>(x.Name.DefaultValue, x.Name.Translations))))
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveProductAttribute(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }
    }
}
