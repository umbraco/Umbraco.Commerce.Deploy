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

using StringExtensions = Umbraco.Commerce.Extensions.StringExtensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.PaymentMethod, UdiType.GuidUdi)]
    public class UmbracoCommercePaymentMethodServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<PaymentMethodArtifact, PaymentMethodReadOnly, PaymentMethod, PaymentMethodState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Payment Methods";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.PaymentMethod;

        public UmbracoCommercePaymentMethodServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(PaymentMethodReadOnly entity)
            => entity.Name;

        public override Task<PaymentMethodReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((PaymentMethodReadOnly?)_umbracoCommerceApi.GetPaymentMethod(id));

        public override IAsyncEnumerable<PaymentMethodReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetPaymentMethods(storeId).ToAsyncEnumerable();

        public override Task<PaymentMethodArtifact?> GetArtifactAsync(GuidUdi? udi, PaymentMethodReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<PaymentMethodArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new PaymentMethodArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Sku = entity.Sku,
                ImageId = entity.ImageId, // Could be a UDI?
                PaymentProviderAlias = entity.PaymentProviderAlias,
                PaymentProviderSettings = new SortedDictionary<string, string>(entity.PaymentProviderSettings
                    .Where(x => !StringExtensions.InvariantContains(_settingsAccessor.Settings.PaymentMethods.IgnoreSettings, x.Key)) // Ignore any settings that shouldn't be transfered
                    .ToDictionary(x => x.Key, x => x.Value)), // Could contain UDIs?
                CanFetchPaymentStatuses = entity.CanFetchPaymentStatuses,
                CanCapturePayments = entity.CanCapturePayments,
                CanCancelPayments = entity.CanCancelPayments,
                CanRefundPayments = entity.CanRefundPayments,
                SortOrder = entity.SortOrder
            };

            // Tax class
            if (entity.TaxClassId != null)
            {
                var taxClassDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.TaxClass, entity.TaxClassId.Value);
                var taxClassDep = new UmbracoCommerceArtifactDependency(taxClassDepUdi);

                dependencies.Add(taxClassDep);

                artifact.TaxClassUdi = taxClassDepUdi;
            }

            // Service prices
            if (entity.Prices.Count > 0)
            {
                var servicesPrices = new List<ServicePriceArtifact>();

                foreach (var price in entity.Prices)
                {
                    var spArtifact = new ServicePriceArtifact { Value = price.Value };

                    // Currency
                    var currencyDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, price.CurrencyId);
                    var currencyDep = new UmbracoCommerceArtifactDependency(currencyDepUdi);

                    dependencies.Add(currencyDep);

                    spArtifact.CurrencyUdi = currencyDepUdi;

                    // Country
                    if (price.CountryId.HasValue)
                    {
                        var countryDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, price.CountryId.Value);
                        var countryDep = new UmbracoCommerceArtifactDependency(countryDepUdi);

                        dependencies.Add(countryDep);

                        spArtifact.CountryUdi = countryDepUdi;
                    }

                    // Region
                    if (price.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Region, price.RegionId.Value);
                        var regionDep = new UmbracoCommerceArtifactDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        spArtifact.RegionUdi = regionDepUdi;
                    }

                    servicesPrices.Add(spArtifact);
                }

                artifact.Prices = servicesPrices;
            }

            // Allowed country regions
            if (entity.AllowedCountryRegions.Count > 0)
            {
                var allowedCountryRegions = new List<AllowedCountryRegionArtifact>();

                foreach (var acr in entity.AllowedCountryRegions)
                {
                    var acrArtifact = new AllowedCountryRegionArtifact();

                    // Country
                    var countryDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, acr.CountryId);
                    var countryDep = new UmbracoCommerceArtifactDependency(countryDepUdi);

                    dependencies.Add(countryDep);

                    acrArtifact.CountryUdi = countryDepUdi;

                    // Region
                    if (acr.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Region, acr.RegionId.Value);
                        var regionDep = new UmbracoCommerceArtifactDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        acrArtifact.RegionUdi = regionDepUdi;
                    }

                    allowedCountryRegions.Add(acrArtifact);
                }

                artifact.AllowedCountryRegions = allowedCountryRegions;
            }

            return Task.FromResult<PaymentMethodArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<PaymentMethodArtifact, PaymentMethodReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<PaymentMethodArtifact, PaymentMethodReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    PaymentMethodArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    PaymentMethod? entity = state.Entity?.AsWritable(uow) ?? PaymentMethod.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        artifact.PaymentProviderAlias);

                    var settings = artifact.PaymentProviderSettings
                        .Where(x => !StringExtensions.InvariantContains(_settingsAccessor.Settings.PaymentMethods.IgnoreSettings, x.Key)) // Ignore any settings that shouldn't be transferred
                        .ToDictionary(x => x.Key, x => x.Value);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetSku(artifact.Sku)
                        .SetImage(artifact.ImageId)
                        .SetSettings(settings, SetBehavior.Merge)
                        .ToggleFeatures(artifact.CanFetchPaymentStatuses, artifact.CanCapturePayments, artifact.CanCancelPayments, artifact.CanRefundPayments)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SavePaymentMethod(entity);

                    state.Entity = entity;

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);

        private Task Pass4Async(ArtifactDeployState<PaymentMethodArtifact, PaymentMethodReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    PaymentMethodArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        PaymentMethod? entity = _umbracoCommerceApi.GetPaymentMethod(state.Entity.Id).AsWritable(uow);

                        // TaxClass
                        if (artifact.TaxClassUdi != null)
                        {
                            artifact.TaxClassUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
                            // TODO: Check the payment method exists?
                            entity.SetTaxClass(artifact.TaxClassUdi.Guid);
                        }
                        else
                        {
                            entity.ClearTaxClass();
                        }

                        // AllowedCountryRegions
                        var allowedCountryRegionsToRemove = entity.AllowedCountryRegions
                            .Where(x => artifact.AllowedCountryRegions == null || !artifact.AllowedCountryRegions.Any(y => y.CountryUdi.Guid == x.CountryId
                                && y.RegionUdi?.Guid == x.RegionId))
                            .ToList();

                        if (artifact.AllowedCountryRegions != null)
                        {
                            foreach (AllowedCountryRegionArtifact acr in artifact.AllowedCountryRegions)
                            {
                                acr.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                                if (acr.RegionUdi != null)
                                {
                                    acr.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);

                                    entity.AllowInRegion(acr.CountryUdi.Guid, acr.RegionUdi.Guid);
                                }
                                else
                                {
                                    entity.AllowInCountry(acr.CountryUdi.Guid);
                                }
                            }
                        }

                        foreach (AllowedCountryRegion? acr in allowedCountryRegionsToRemove)
                        {
                            if (acr.RegionId != null)
                            {
                                entity.DisallowInRegion(acr.CountryId, acr.RegionId.Value);
                            }
                            else
                            {
                                entity.DisallowInCountry(acr.CountryId);
                            }
                        }

                        // Prices
                        // Must come after AllowedCountryRegions as it may be affected by them
                        var pricesToRemove = entity.Prices
                            .Where(x => artifact.Prices == null || !artifact.Prices.Any(y => y.CountryUdi?.Guid == x.CountryId
                                && y.RegionUdi?.Guid == x.RegionId
                                && y.CurrencyUdi.Guid == x.CurrencyId))
                            .ToList();

                        if (artifact.Prices != null)
                        {
                            foreach (ServicePriceArtifact price in artifact.Prices)
                            {
                                price.CurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);

                                if (price is { CountryUdi: null, RegionUdi: null })
                                {
                                    entity.SetDefaultPriceForCurrency(price.CurrencyUdi.Guid, price.Value);
                                }
                                else
                                {
                                    price.CountryUdi!.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                                    if (price.RegionUdi != null)
                                    {
                                        price.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);

                                        entity.SetRegionPriceForCurrency(price.CountryUdi.Guid, price.RegionUdi.Guid, price.CurrencyUdi.Guid, price.Value);
                                    }
                                    else
                                    {
                                        entity.SetCountryPriceForCurrency(price.CountryUdi.Guid, price.CurrencyUdi.Guid, price.Value);
                                    }
                                }
                            }
                        }

                        foreach (ServicePrice? price in pricesToRemove)
                        {
                            switch (price)
                            {
                                case { CountryId: null, RegionId: null }:
                                    entity.ClearDefaultPriceForCurrency(price.CurrencyId);
                                    break;
                                case { CountryId: not null, RegionId: null }:
                                    entity.ClearCountryPriceForCurrency(price.CountryId.Value, price.CurrencyId);
                                    break;
                                default:
                                    entity.ClearRegionPriceForCurrency(price.CountryId!.Value, price.RegionId!.Value, price.CurrencyId);
                                    break;
                            }
                        }

                        _umbracoCommerceApi.SavePaymentMethod(entity);
                    }

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
