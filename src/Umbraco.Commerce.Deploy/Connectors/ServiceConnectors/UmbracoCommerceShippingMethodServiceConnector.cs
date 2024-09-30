using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Core;

using StringExtensions = Umbraco.Commerce.Extensions.StringExtensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, UdiType.GuidUdi)]
    public class UmbracoCommerceShippingMethodServiceConnector(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor,
        IOptionsMonitor<JsonOptions> jsonOptions)
        : UmbracoCommerceStoreEntityServiceConnectorBase<ShippingMethodArtifact, ShippingMethodReadOnly, ShippingMethod,
            ShippingMethodState>(umbracoCommerceApi, settingsAccessor)
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = jsonOptions.Get(DeployConstants.JsonOptionsNames.Deploy).JsonSerializerOptions;

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

        protected override string OpenUdiName => "All Umbraco Commerce Shipping Methods";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ShippingMethod;

        public override string GetEntityName(ShippingMethodReadOnly entity)
            => entity.Name;

        public override Task<ShippingMethodReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((ShippingMethodReadOnly?)_umbracoCommerceApi.GetShippingMethod(id));

        public override IAsyncEnumerable<ShippingMethodReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetShippingMethods(storeId).ToAsyncEnumerable();

        public override Task<ShippingMethodArtifact?> GetArtifactAsync(GuidUdi? udi, ShippingMethodReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<ShippingMethodArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new ShippingMethodArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Sku = entity.Sku,
                ImageId = entity.ImageId, // Could be a UDI?
                CalculationMode = (int)entity.CalculationMode,
                ShippingProviderAlias = entity.ShippingProviderAlias,
                ShippingProviderSettings = new SortedDictionary<string, string>(entity.ShippingProviderSettings
                    .Where(x => !StringExtensions.InvariantContains(_settingsAccessor.Settings.ShippingMethods.IgnoreSettings, x.Key)) // Ignore any settings that shouldn't be transfered
                    .ToDictionary(x => x.Key, x => x.Value)), // Could contain UDIs?
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

            // Only the fixed rate shipping provider has delcared dependencies
            // that we can deserialize. Dynamic can have a dependency, but because
            // it's config is all plugin based, we can't be certain of the data structure
            // and so we just have to pass the value through. Realtime rates don't currently
            // have any dependencies.
            if (entity.CalculationConfig != null)
            {
                if (entity is { CalculationMode: ShippingCalculationMode.Fixed, CalculationConfig: FixedRateShippingCalculationConfig calcConfig })
                {
                    var servicesPrices = new List<ServicePriceArtifact>();

                    foreach (ServicePrice? price in calcConfig.Prices)
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

                    artifact.CalculationConfig = JsonSerializer.SerializeToElement(new FixedRateShippingCalculationConfigArtifact
                    {
                        Prices = servicesPrices
                    }, _jsonSerializerOptions);
                }
                else
                {
                    // No additional processing required
                    artifact.CalculationConfig = JsonSerializer.SerializeToElement(entity.CalculationConfig, _jsonSerializerOptions);
                }
            }

            // Allowed country regions
            if (entity.AllowedCountryRegions.Count > 0)
            {
                var allowedCountryRegions = new List<AllowedCountryRegionArtifact>();

                foreach (var acr in entity.AllowedCountryRegions.OrderBy(x => x.CountryId).ThenBy(x => x.RegionId))
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

            return Task.FromResult<ShippingMethodArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    ShippingMethodArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    ShippingMethod? entity = state.Entity?.AsWritable(uow) ?? ShippingMethod.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        artifact.ShippingProviderAlias,
                        (ShippingCalculationMode)artifact.CalculationMode);

                    var settings = artifact.ShippingProviderSettings
                        .Where(x => !StringExtensions.InvariantContains(_settingsAccessor.Settings.ShippingMethods.IgnoreSettings, x.Key)) // Ignore any settings that shouldn't be transfered
                        .ToDictionary(x => x.Key, x => x.Value);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetSku(artifact.Sku)
                        .SetImage(artifact.ImageId)
                        .SetSettings(settings, SetBehavior.Merge)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveShippingMethod(entity);

                    state.Entity = entity;

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);

        private Task Pass4Async(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    ShippingMethodArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        ShippingMethod? entity = _umbracoCommerceApi.GetShippingMethod(state.Entity.Id).AsWritable(uow);

                        // TaxClass
                        if (artifact.TaxClassUdi != null)
                        {
                            artifact.TaxClassUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);

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

                        // Calculation config
                        // Must come after AllowedCountryRegions as it may depend on them
                        if (artifact.CalculationConfig != null)
                        {
                            if (artifact.CalculationMode == (int)ShippingCalculationMode.Fixed)
                            {
                                FixedRateShippingCalculationConfigArtifact? cfgArtifact = artifact.CalculationConfig?.Deserialize<FixedRateShippingCalculationConfigArtifact>(_jsonSerializerOptions);

                                var prices = new List<ServicePrice>();

                                foreach (ServicePriceArtifact price in cfgArtifact.Prices)
                                {
                                    price.CurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);

                                    if (price.CountryUdi != null)
                                    {
                                        price.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);
                                    }

                                    if (price.RegionUdi != null)
                                    {
                                        price.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);
                                    }

                                    prices.Add(new ServicePrice(price.Value, price.CurrencyUdi.Guid, price.CountryUdi?.Guid, price.RegionUdi?.Guid));
                                }

                                entity.SetCalculationConfig(new FixedRateShippingCalculationConfig(prices));
                            }
                            else if (artifact.CalculationMode == (int)ShippingCalculationMode.Dynamic)
                            {
                                entity.SetCalculationConfig(artifact.CalculationConfig?.Deserialize<DynamicRateShippingCalculationConfig>(_jsonSerializerOptions));
                            }
                            else if (artifact.CalculationMode == (int)ShippingCalculationMode.Realtime)
                            {
                                entity.SetCalculationConfig(artifact.CalculationConfig?.Deserialize<RealtimeRateShippingCalculationConfig>(_jsonSerializerOptions));
                            }
                            else
                            {
                                throw new ApplicationException($"Unknown calculation mode: {artifact.CalculationMode}");
                            }
                        }

                        _umbracoCommerceApi.SaveShippingMethod(entity);
                    }

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
