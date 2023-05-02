using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Region, UdiType.GuidUdi)]
    public class UmbracoCommerceRegionServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<RegionArtifact, RegionReadOnly, Region, RegionState>
    {
        public override int[] ProcessPasses => new[]
        {
            3,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Umbraco Commerce Regions";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Region;

        public UmbracoCommerceRegionServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(RegionReadOnly entity)
            => entity.Name;

        public override RegionReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetRegion(id);

        public override IEnumerable<RegionReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetRegions(storeId);

        public override RegionArtifact GetArtifact(GuidUdi udi, RegionReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);
            var countryUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, entity.CountryId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi),
                new UmbracoCommerceArtifactDependency(countryUdi)
            };

            var artifcat = new RegionArtifact(udi, storeUdi, countryUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                SortOrder = entity.SortOrder
            };

            // Default payment method
            if (entity.DefaultPaymentMethodId != null)
            {
                var pmDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.PaymentMethod, entity.DefaultPaymentMethodId.Value);
                var pmDep = new UmbracoCommerceArtifactDependency(pmDepUdi);

                dependencies.Add(pmDep);

                artifcat.DefaultPaymentMethodUdi = pmDepUdi;
            }

            // Default shipping method
            if (entity.DefaultShippingMethodId != null)
            {
                var smDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, entity.DefaultShippingMethodId.Value);
                var smDep = new UmbracoCommerceArtifactDependency(smDepUdi);

                dependencies.Add(smDep);

                artifcat.DefaultShippingMethodUdi = smDepUdi;
            }

            return artifcat;
        }

        public override void Process(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 3:
                    Pass3(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass3(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);
                artifact.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                var entity = state.Entity?.AsWritable(uow) ?? Region.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.CountryUdi.Guid, artifact.Code, artifact.Name);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveRegion(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetRegion(state.Entity.Id).AsWritable(uow);

                if (artifact.DefaultPaymentMethodUdi != null)
                {
                    artifact.DefaultPaymentMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultPaymentMethod(artifact.DefaultPaymentMethodUdi?.Guid);

                if (artifact.DefaultShippingMethodUdi != null)
                {
                    artifact.DefaultShippingMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultShippingMethod(artifact.DefaultShippingMethodUdi?.Guid);

                _umbracoCommerceApi.SaveRegion(entity);

                uow.Complete();
            });
        }
    }
}
