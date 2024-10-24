using System;
using System.Collections.Generic;
using System.Linq;
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Tax Classes";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.TaxClass;

        public UmbracoCommerceTaxClassServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(TaxClassReadOnly entity)
            => entity.Name;

        public override TaxClassReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetTaxClass(id);

        public override IEnumerable<TaxClassReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetTaxClasses(storeId);

        public override TaxClassArtifact GetArtifact(GuidUdi udi, TaxClassReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifcat = new TaxClassArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                DefaultTaxRate = entity.DefaultTaxRate,
                SortOrder = entity.SortOrder
            };

            // Country region tax rates
            var countryRegionTaxRateArtifacts = new List<CountryRegionTaxRateArtifact>();

            foreach (var countryRegionTaxRate in entity.CountryRegionTaxRates)
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
                    var regionDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Region, countryRegionTaxRate.RegionId.Value);
                    var regionDep = new UmbracoCommerceArtifactDependency(regionDepUdi);
                    dependencies.Add(regionDep);

                    crtrArtifact.RegionUdi = regionDepUdi;
                }

                countryRegionTaxRateArtifacts.Add(crtrArtifact);
            }

            artifcat.CountryRegionTaxRates = countryRegionTaxRateArtifacts;

            return artifcat;
        }

        public override void Process(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? TaxClass.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name, artifact.DefaultTaxRate);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetDefaultTaxRate(artifact.DefaultTaxRate)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveTaxClass(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetTaxClass(state.Entity.Id).AsWritable(uow);

                // Should probably validate the entity type here too, but really
                // given we are using guids, the likelyhood of a matching guid
                // being for a different entity type are pretty slim
                var countryRegionTaxRatesToRemove = entity.CountryRegionTaxRates
                    .Where(x => artifact.CountryRegionTaxRates == null || !artifact.CountryRegionTaxRates.Any(y => y.CountryUdi.Guid == x.CountryId && y.RegionUdi?.Guid == x.RegionId))
                    .ToList();

                if (artifact.CountryRegionTaxRates != null)
                {
                    foreach (var crtr in artifact.CountryRegionTaxRates)
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

                foreach (var crtr in countryRegionTaxRatesToRemove)
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

                uow.Complete();
            });
        }
    }
}
