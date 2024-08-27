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

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.TaxClass, UdiType.GuidUdi)]
    public class UmbracoCommerceTaxClassServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<TaxClassArtifact, TaxClassReadOnly, TaxClass, TaxClassState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Tax Classes";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.TaxClass;

        public UmbracoCommerceTaxClassServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(TaxClassReadOnly entity)
            => entity.Name;

        public override Task<TaxClassReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((TaxClassReadOnly?)_umbracoCommerceApi.GetTaxClass(id));

        public override IAsyncEnumerable<TaxClassReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetTaxClasses(storeId).ToAsyncEnumerable();

        public override Task<TaxClassArtifact?> GetArtifactAsync(GuidUdi? udi, TaxClassReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<TaxClassArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new TaxClassArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                DefaultTaxRate = entity.DefaultTaxRate,
                SortOrder = entity.SortOrder
            };

            // Country region tax rates
            var countryRegionTaxRateArtifacts = new List<CountryRegionTaxRateArtifact>();

            foreach (var countryRegionTaxRate in entity.CountryRegionTaxRates.OrderBy(x => x.CountryId).ThenBy(x => x.RegionId))
            {
                var crtrArtifact = new CountryRegionTaxRateArtifact
                {
                    TaxRate = countryRegionTaxRate.TaxRate
                };

                var countryDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, countryRegionTaxRate.CountryId);
                var countryDep = new UmbracoCommerceArtifactDependency(countryDepUdi);
                dependencies.Add(countryDep);

                crtrArtifact.CountryUdi = countryDepUdi;

                if (countryRegionTaxRate.RegionId.HasValue)
                {
                    var regionDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, countryRegionTaxRate.CountryId);
                    var regionDep = new UmbracoCommerceArtifactDependency(regionDepUdi);
                    dependencies.Add(regionDep);

                    crtrArtifact.RegionUdi = regionDepUdi;
                }

                countryRegionTaxRateArtifacts.Add(crtrArtifact);
            }

            artifact.CountryRegionTaxRates = countryRegionTaxRateArtifacts;

            return Task.FromResult<TaxClassArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    TaxClassArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    TaxClass? entity = state.Entity?.AsWritable(uow) ?? TaxClass.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        artifact.DefaultTaxRate);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetDefaultTaxRate(artifact.DefaultTaxRate)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveTaxClass(entity);

                    state.Entity = entity;

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);

        private Task Pass4Async(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    TaxClassArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        TaxClass? entity = _umbracoCommerceApi.GetTaxClass(state.Entity.Id).AsWritable(uow);

                        // Should probably validate the entity type here too, but really
                        // given we are using guids, the likelyhood of a matching guid
                        // being for a different entity type are pretty slim
                        var countryRegionTaxRatesToRemove = entity.CountryRegionTaxRates
                            .Where(x => artifact.CountryRegionTaxRates == null || !artifact.CountryRegionTaxRates.Any(y => y.CountryUdi.Guid == x.CountryId && y.RegionUdi?.Guid == x.RegionId))
                            .ToList();

                        if (artifact.CountryRegionTaxRates != null)
                        {
                            foreach (CountryRegionTaxRateArtifact crtr in artifact.CountryRegionTaxRates)
                            {
                                crtr.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                                if (crtr.RegionUdi == null)
                                {
                                    entity.SetCountryTaxRate(crtr.CountryUdi.Guid, crtr.TaxRate);
                                }
                                else
                                {
                                    crtr.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);

                                    entity.SetRegionTaxRate(crtr.CountryUdi.Guid, crtr.RegionUdi.Guid, crtr.TaxRate);
                                }
                            }
                        }

                        foreach (CountryRegionTaxRate? crtr in countryRegionTaxRatesToRemove)
                        {
                            if (crtr.RegionId == null)
                            {
                                entity.ClearCountryTaxRate(crtr.CountryId);
                            }
                            else
                            {
                                entity.ClearRegionTaxRate(crtr.CountryId, crtr.RegionId.Value);
                            }
                        }

                        _umbracoCommerceApi.SaveTaxClass(entity);
                    }

                    uow.Complete();

                    return Task.CompletedTask;

                },
                cancellationToken);
    }
}
