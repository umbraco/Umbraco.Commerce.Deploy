using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Country, UdiType.GuidUdi)]
    public class UmbracoCommerceCountryServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<CountryArtifact, CountryReadOnly, Country, CountryState>
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Countries";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Country;

        public UmbracoCommerceCountryServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(CountryReadOnly entity)
            => entity.Name;

        public override CountryReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetCountry(id);

        public override IEnumerable<CountryReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetCountries(storeId);

        public override CountryArtifact GetArtifact(GuidUdi udi, CountryReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            var artifcat = new CountryArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                SortOrder = entity.SortOrder
            };

            // Default currency
            if (entity.DefaultCurrencyId != null)
            {
                var currencyDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, entity.DefaultCurrencyId.Value);
                var currencyDep = new UmbracoCommerceArtifactDependency(currencyDepUdi);

                dependencies.Add(currencyDep);

                artifcat.DefaultCurrencyUdi = currencyDepUdi;
            }

            // Default payment method
            if (entity.DefaultPaymentMethodId != null)
            {
                var pmDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.PaymentMethod, entity.DefaultPaymentMethodId.Value);
                var pmDep = new UmbracoCommerceArtifactDependency(pmDepUdi);

                dependencies.Add(pmDep);

                artifcat.DefaultPaymentMethodUdi = pmDepUdi;
            }

            // Default shipping method
            if (entity.DefaultShippingMethodId != null)
            {
                var smDepUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, entity.DefaultShippingMethodId.Value);
                var smDep = new UmbracoCommerceArtifactDependency(smDepUdi);

                dependencies.Add(smDep);

                artifcat.DefaultShippingMethodUdi = smDepUdi;
            }

            return artifcat;
        }

        public override void Process(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? Country.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Code, artifact.Name);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveCountry(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetCountry(state.Entity.Id).AsWritable(uow);

                if (artifact.DefaultCurrencyUdi != null)
                {
                    artifact.DefaultCurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);
                    // TODO: Check the currency exists?
                }

                entity.SetDefaultCurrency(artifact.DefaultCurrencyUdi?.Guid);

                if (artifact.DefaultPaymentMethodUdi != null)
                {
                    artifact.DefaultPaymentMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultPaymentMethod(artifact.DefaultPaymentMethodUdi?.Guid);

                if (artifact.DefaultShippingMethodUdi != null)
                {
                    artifact.DefaultShippingMethodUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultShippingMethod(artifact.DefaultShippingMethodUdi?.Guid);

                _umbracoCommerceApi.SaveCountry(entity);

                uow.Complete();

            });
        }
    }
}
