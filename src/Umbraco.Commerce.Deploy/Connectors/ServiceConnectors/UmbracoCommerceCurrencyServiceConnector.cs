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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Currency, UdiType.GuidUdi)]
    public class UmbracoCommerceCurrencyServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<CurrencyArtifact, CurrencyReadOnly, Currency, CurrencyState>
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

        protected override string OpenUdiName  => "All Umbraco Commerce Currencies";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Currency;

        public UmbracoCommerceCurrencyServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(CurrencyReadOnly entity)
            => entity.Name;

        public override Task<CurrencyReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetCurrencyAsync(id);

        public override IAsyncEnumerable<CurrencyReadOnly> GetEntitiesAsync(
            Guid storeId,
            CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetCurrenciesAsync(storeId).AsAsyncEnumerable();

        public override Task<CurrencyArtifact?> GetArtifactAsync(GuidUdi? udi, CurrencyReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<CurrencyArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifact = new CurrencyArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                CultureName = entity.CultureName,
                FormatTemplate = entity.FormatTemplate,
                SortOrder = entity.SortOrder
            };

            // Allowed countries
            if (entity.AllowedCountries.Count > 0)
            {
                var allowedCountryArtifacts = new List<AllowedCountryArtifact>();

                foreach (AllowedCountry? allowedCountry in entity.AllowedCountries.OrderBy(x => x.CountryId))
                {
                    var countryDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, allowedCountry.CountryId);
                    var countryDep = new UmbracoCommerceArtifactDependency(countryDepUdi);

                    dependencies.Add(countryDep);

                    allowedCountryArtifacts.Add(new AllowedCountryArtifact { CountryUdi = countryDepUdi });
                }

                artifact.AllowedCountries = allowedCountryArtifacts;
            }

            return Task.FromResult<CurrencyArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private async Task Pass2Async(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    CurrencyArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    Currency? entity = await state.Entity?.AsWritableAsync(uow)! ?? await Currency.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Code,
                        artifact.Name,
                        artifact.CultureName);

                    await entity.SetNameAsync(artifact.Name)
                        .SetCodeAsync(artifact.Code)
                        .SetCultureAsync(artifact.CultureName)
                        .SetCustomFormatTemplateAsync(artifact.FormatTemplate)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveCurrencyAsync(entity, ct);

                    state.Entity = entity;

                    await uow.CompleteAsync();
                },
                cancellationToken);

        private async Task Pass4Async(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    CurrencyArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        Currency? entity = await _umbracoCommerceApi.GetCurrencyAsync(state.Entity.Id).AsWritableAsync(uow);

                        var allowedCountriesToRemove = entity.AllowedCountries
                            .Where(x => artifact.AllowedCountries == null || artifact.AllowedCountries.All(y => y.CountryUdi.Guid != x.CountryId))
                            .ToList();

                        if (artifact.AllowedCountries != null)
                        {
                            foreach (AllowedCountryArtifact ac in artifact.AllowedCountries)
                            {
                                ac.CountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                                await entity.AllowInCountryAsync(ac.CountryUdi.Guid);
                            }
                        }

                        foreach (AllowedCountry? ac in allowedCountriesToRemove)
                        {
                            await entity.DisallowInCountryAsync(ac.CountryId);
                        }

                        await _umbracoCommerceApi.SaveCurrencyAsync(entity, ct);
                    }

                    await uow.CompleteAsync();
                },
                cancellationToken);
    }
}
