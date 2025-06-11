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
            => _umbracoCommerceApi.GetTaxClassAsync(id);

        public override IAsyncEnumerable<TaxClassReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetTaxClassesAsync(storeId).AsAsyncEnumerable();

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
                DefaultTaxCode = entity.DefaultTaxCode,
                SortOrder = entity.SortOrder
            };

            // Country region tax rates
            var countryRegionTaxRateArtifacts = new List<CountryRegionTaxClassArtifact>();

            foreach (var countryRegionTaxClass in entity.CountryRegionTaxClasses.OrderBy(x => x.CountryId).ThenBy(x => x.RegionId))
            {
                var crtrArtifact = new CountryRegionTaxClassArtifact
                {
                    TaxRate = countryRegionTaxClass.TaxRate,
                    TaxCode = countryRegionTaxClass.TaxCode
                };

                var countryDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, countryRegionTaxClass.CountryId);
                var countryDep = new UmbracoCommerceArtifactDependency(countryDepUdi);
                dependencies.Add(countryDep);

                crtrArtifact.CountryUdi = countryDepUdi;

                if (countryRegionTaxClass.RegionId.HasValue)
                {
                    var regionDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Region, countryRegionTaxClass.RegionId.Value);
                    var regionDep = new UmbracoCommerceArtifactDependency(regionDepUdi);
                    dependencies.Add(regionDep);

                    crtrArtifact.RegionUdi = regionDepUdi;
                }

                countryRegionTaxRateArtifacts.Add(crtrArtifact);
            }

            artifact.CountryRegionTaxClasses = countryRegionTaxRateArtifacts;

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

        private async Task Pass2Async(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    TaxClassArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    TaxClass? entity = state.Entity != null ? await state.Entity.AsWritableAsync(uow) : await TaxClass.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        artifact.DefaultTaxRate);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetDefaultTaxRateAsync(artifact.DefaultTaxRate)
                        .SetDefaultTaxCodeAsync(artifact.DefaultTaxCode)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveTaxClassAsync(entity, ct);

                    state.Entity = entity;

                    uow.Complete();
                },
                cancellationToken);

        private async Task Pass4Async(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    TaxClassArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        TaxClass? entity = await _umbracoCommerceApi.GetTaxClassAsync(state.Entity.Id).AsWritableAsync(uow);

                        // Should probably validate the entity type here too, but really
                        // given we are using guids, the likelyhood of a matching guid
                        // being for a different entity type are pretty slim
                        var countryRegionTaxClassesToRemove = entity.CountryRegionTaxClasses
                            .Where(x => artifact.CountryRegionTaxClasses == null || !artifact.CountryRegionTaxClasses.Any(y => y.CountryUdi.Guid == x.CountryId && y.RegionUdi?.Guid == x.RegionId))
                            .ToList();

                        if (artifact.CountryRegionTaxClasses != null)
                        {
                            foreach (CountryRegionTaxClassArtifact crtr in artifact.CountryRegionTaxClasses)
                            {
                                crtr.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                                if (crtr.RegionUdi == null)
                                {
                                    await entity.SetCountryTaxClassAsync(crtr.CountryUdi.Guid, crtr.TaxRate, crtr.TaxCode);
                                }
                                else
                                {
                                    crtr.RegionUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Region);

                                    await entity.SetRegionTaxClassAsync(crtr.CountryUdi.Guid, crtr.RegionUdi.Guid, crtr.TaxRate, crtr.TaxCode);
                                }
                            }
                        }

                        foreach (CountryRegionTaxClass? crtr in countryRegionTaxClassesToRemove)
                        {
                            if (crtr.RegionId == null)
                            {
                                await entity.ClearCountryTaxClassAsync(crtr.CountryId);
                            }
                            else
                            {
                                await entity.ClearRegionTaxClassAsync(crtr.CountryId, crtr.RegionId.Value);
                            }
                        }

                        await _umbracoCommerceApi.SaveTaxClassAsync(entity, ct);
                    }

                    uow.Complete();
                },
                cancellationToken);
    }
}
