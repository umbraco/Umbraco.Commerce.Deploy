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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod, UdiType.GuidUdi)]
    public class UmbracoCommerceTaxCalculationMethodServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<TaxCalculationMethodArtifact, TaxCalculationMethodReadOnly, TaxCalculationMethod, TaxCalculationMethodState>
    {
        protected override int[] ProcessPasses => new[]
        {
            2
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All Umbraco Commerce Tax Calculation Methods";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod;

        public UmbracoCommerceTaxCalculationMethodServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(TaxCalculationMethodReadOnly entity)
            => entity.Name;

        public override Task<TaxCalculationMethodReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetTaxCalculationMethodAsync(id);

        public override IAsyncEnumerable<TaxCalculationMethodReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetTaxCalculationMethodsAsync(storeId).AsAsyncEnumerable();

        public override Task<TaxCalculationMethodArtifact?> GetArtifactAsync(GuidUdi? udi, TaxCalculationMethodReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<TaxCalculationMethodArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new TaxCalculationMethodArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                SalesTaxProviderAlias = entity.SalesTaxProviderAlias,
                SalesTaxProviderSettings = new SortedDictionary<string, string>(entity.SalesTaxProviderSettings
                    .Where(x => !StringExtensions.InvariantContains(_settingsAccessor.Settings.TaxCalculationMethods.IgnoreSettings, x.Key)) // Ignore any settings that shouldn't be transfered
                    .ToDictionary(x => x.Key, x => x.Value)), // Could contain UDIs?
                SortOrder = entity.SortOrder
            };

            return Task.FromResult<TaxCalculationMethodArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<TaxCalculationMethodArtifact, TaxCalculationMethodReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 2:
                    await Pass2Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private async Task Pass2Async(ArtifactDeployState<TaxCalculationMethodArtifact, TaxCalculationMethodReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    TaxCalculationMethodArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    TaxCalculationMethod? entity = state.Entity != null ? await state.Entity.AsWritableAsync(uow) : await TaxCalculationMethod.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        artifact.SalesTaxProviderAlias);

                    var settings = artifact.SalesTaxProviderSettings
                        .Where(x => !_settingsAccessor.Settings.TaxCalculationMethods.IgnoreSettings.InvariantContains(x.Key)) // Ignore any settings that shouldn't be transferred
                        .ToDictionary(x => x.Key, x => x.Value);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetSettingsAsync(settings, SetBehavior.Merge)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveTaxCalculationMethodAsync(entity, ct);

                    state.Entity = entity;

                    uow.Complete();
                },
                cancellationToken);
    }
}
