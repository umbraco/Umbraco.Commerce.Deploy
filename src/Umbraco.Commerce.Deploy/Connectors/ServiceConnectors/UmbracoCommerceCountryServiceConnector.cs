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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Country, UdiType.GuidUdi)]
    public class UmbracoCommerceCountryServiceConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        : UmbracoCommerceStoreEntityServiceConnectorBase<CountryArtifact, CountryReadOnly, Country, CountryState>(
            umbracoCommerceApi, settingsAccessor)
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

        protected override string OpenUdiName => "All Umbraco Commerce Countries";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Country;

        public override string GetEntityName(CountryReadOnly entity)
            => entity.Name;

        public override Task<CountryReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetCountryAsync(id);

        public override IAsyncEnumerable<CountryReadOnly> GetEntitiesAsync(
            Guid storeId,
            CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetCountriesAsync(storeId).AsAsyncEnumerable();

        public override Task<CountryArtifact?> GetArtifactAsync(GuidUdi? udi, CountryReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<CountryArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi),
            };

            var artifact = new CountryArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                SortOrder = entity.SortOrder
            };

            // Tax calculation method
            if (entity.TaxCalculationMethodId != null)
            {
                var taxCalculationMethodDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod, entity.TaxCalculationMethodId.Value);
                var taxCalculationMethodDep = new UmbracoCommerceArtifactDependency(taxCalculationMethodDepUdi);

                dependencies.Add(taxCalculationMethodDep);

                artifact.TaxCalculationMethodUdi = taxCalculationMethodDepUdi;
            }

            // Default currency
            if (entity.DefaultCurrencyId != null)
            {
                var currencyDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, entity.DefaultCurrencyId.Value);
                var currencyDep = new UmbracoCommerceArtifactDependency(currencyDepUdi);

                dependencies.Add(currencyDep);

                artifact.DefaultCurrencyUdi = currencyDepUdi;
            }

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

            return Task.FromResult<CountryArtifact?>(artifact);
        }

        public override async Task ProcessAsync(
            ArtifactDeployState<CountryArtifact, CountryReadOnly> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
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

        private async Task Pass2Async(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    CountryArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    Country? entity = await state.Entity?.AsWritableAsync(uow)! ?? await Country.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Code,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name)
                        .SetCodeAsync(artifact.Code)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveCountryAsync(entity, ct);

                    state.Entity = entity;

                    await uow.CompleteAsync();
                },
                cancellationToken);

        private async Task Pass4Async(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    CountryArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        Country? entity = await _umbracoCommerceApi.GetCountryAsync(state.Entity.Id).AsWritableAsync(uow);

                        if (artifact.TaxCalculationMethodUdi != null)
                        {
                            artifact.TaxCalculationMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod);
                            // TODO: Check the tax calculation method exists?
                        }

                        await entity.SetTaxCalculationMethodAsync(artifact.TaxCalculationMethodUdi?.Guid);

                        if (artifact.DefaultCurrencyUdi != null)
                        {
                            artifact.DefaultCurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);
                            // TODO: Check the currency exists?
                        }

                        await entity.SetDefaultCurrencyAsync(artifact.DefaultCurrencyUdi?.Guid);

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

                        await _umbracoCommerceApi.SaveCountryAsync(entity, ct);
                    }

                    await uow.CompleteAsync();
                },
                cancellationToken);
    }
}
