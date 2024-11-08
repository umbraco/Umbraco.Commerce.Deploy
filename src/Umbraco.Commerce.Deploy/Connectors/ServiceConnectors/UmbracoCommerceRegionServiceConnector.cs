using System;
using System.Collections.Generic;
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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Region, UdiType.GuidUdi)]
    public class UmbracoCommerceRegionServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<RegionArtifact, RegionReadOnly, Region, RegionState>
    {
        protected override int[] ProcessPasses => new[]
        {
            3,4
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All Umbraco Commerce Regions";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Region;

        public UmbracoCommerceRegionServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(RegionReadOnly entity)
            => entity.Name;

        public override Task<RegionReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetRegionAsync(id);

        public override IAsyncEnumerable<RegionReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetRegionsAsync(storeId).AsAsyncEnumerable();

        public override Task<RegionArtifact?> GetArtifactAsync(GuidUdi? udi, RegionReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<RegionArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);
            var countryUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, entity.CountryId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi),
                new UmbracoCommerceArtifactDependency(countryUdi)
            };

            var artifact = new RegionArtifact(udi, storeUdi, countryUdi, dependencies)
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

                artifact.DefaultPaymentMethodUdi = pmDepUdi;
            }

            // Default shipping method
            if (entity.DefaultShippingMethodId != null)
            {
                var smDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, entity.DefaultShippingMethodId.Value);
                var smDep = new UmbracoCommerceArtifactDependency(smDepUdi);

                dependencies.Add(smDep);

                artifact.DefaultShippingMethodUdi = smDepUdi;
            }

            return Task.FromResult<RegionArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 3:
                    await Pass3Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                case 4:
                    await Pass4Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private async Task Pass3Async(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    RegionArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);
                    artifact.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                    Region? entity = await state.Entity?.AsWritableAsync(uow)! ?? await Region.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.CountryUdi.Guid,
                        artifact.Code,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name)
                        .SetCodeAsync(artifact.Code)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveRegionAsync(entity, ct);

                    state.Entity = entity;

                    uow.Complete();
                },
                cancellationToken);

        private async Task Pass4Async(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    RegionArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        Region? entity = await _umbracoCommerceApi.GetRegionAsync(state.Entity.Id).AsWritableAsync(uow);

                        if (artifact.DefaultPaymentMethodUdi != null)
                        {
                            artifact.DefaultPaymentMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
                            // TODO: Check the payment method exists?
                        }

                        await entity.SetDefaultPaymentMethodAsync(artifact.DefaultPaymentMethodUdi?.Guid);

                        if (artifact.DefaultShippingMethodUdi != null)
                        {
                            artifact.DefaultShippingMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
                            // TODO: Check the payment method exists?
                        }

                        await entity.SetDefaultShippingMethodAsync(artifact.DefaultShippingMethodUdi?.Guid);

                        await _umbracoCommerceApi.SaveRegionAsync(entity, ct);
                    }

                    uow.Complete();
                },
                cancellationToken);
    }
}
