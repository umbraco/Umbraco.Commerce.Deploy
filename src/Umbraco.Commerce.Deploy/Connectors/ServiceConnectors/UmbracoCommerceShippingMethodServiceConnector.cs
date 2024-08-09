using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

using StringExtensions = Umbraco.Commerce.Extensions.StringExtensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, UdiType.GuidUdi)]
    public class UmbracoCommerceShippingMethodServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ShippingMethodArtifact, ShippingMethodReadOnly, ShippingMethod, ShippingMethodState>
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Shipping Methods";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ShippingMethod;

        public UmbracoCommerceShippingMethodServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ShippingMethodReadOnly entity)
            => entity.Name;

        public override ShippingMethodReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetShippingMethod(id);

        public override IEnumerable<ShippingMethodReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetShippingMethods(storeId);

        public override ShippingMethodArtifact GetArtifact(GuidUdi udi, ShippingMethodReadOnly entity)
        {
            if (entity == null)
                return null;

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
                if (entity.CalculationMode == ShippingCalculationMode.Fixed && entity.CalculationConfig is FixedRateShippingCalculationConfig calcConfig)
                {
                    var servicesPrices = new List<ServicePriceArtifact>();

                    foreach (var price in calcConfig.Prices)
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

                    artifact.CalculationConfig = JObject.FromObject(new FixedRateShippingCalculationConfigArtifact
                    {
                        Prices = servicesPrices
                    });
                }
                else
                {
                    // No additional processing required
                    artifact.CalculationConfig = JObject.FromObject(entity.CalculationConfig);
                }
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

            return artifact;
        }

        public override void Process(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ShippingMethod.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name, artifact.ShippingProviderAlias, (ShippingCalculationMode)artifact.CalculationMode);

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
            });
        }

        private void Pass4(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetShippingMethod(state.Entity.Id).AsWritable(uow);

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
                    foreach (var acr in artifact.AllowedCountryRegions)
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

                foreach (var acr in allowedCountryRegionsToRemove)
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
                if (artifact.CalculationConfig != null)
                {
                    if (artifact.CalculationMode == (int)ShippingCalculationMode.Fixed)
                    {
                        var cfgArtifact = artifact.CalculationConfig.ToObject<FixedRateShippingCalculationConfigArtifact>();
                        var prices = new List<ServicePrice>();

                        foreach (var price in cfgArtifact.Prices)
                        {
                            price.CurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);

                            if (price.CountryUdi != null)
                                price.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                            if (price.RegionUdi != null)
                                price.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);

                            prices.Add(new ServicePrice(price.Value, price.CurrencyUdi.Guid, price.CountryUdi?.Guid, price.RegionUdi?.Guid));
                        }

                        entity.SetCalculationConfig(new FixedRateShippingCalculationConfig(prices));
                    }
                    else if (artifact.CalculationMode == (int)ShippingCalculationMode.Dynamic)
                    {
                        entity.SetCalculationConfig(artifact.CalculationConfig.ToObject<DynamicRateShippingCalculationConfig>());
                    }
                    else if (artifact.CalculationMode == (int)ShippingCalculationMode.Realtime)
                    {
                        entity.SetCalculationConfig(artifact.CalculationConfig.ToObject<RealtimeRateShippingCalculationConfig>());
                    }
                    else
                    {
                        throw new ApplicationException($"Unknown calculation mode: {artifact.CalculationMode}");
                    }
                }

                _umbracoCommerceApi.SaveShippingMethod(entity);

                uow.Complete();
            });
        }
    }
}
