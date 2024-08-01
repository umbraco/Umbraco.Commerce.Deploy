using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Store, UdiType.GuidUdi)]
    public class UmbracoCommerceStoreServiceConnector : UmbracoCommerceEntityServiceConnectorBase<StoreArtifact, StoreReadOnly>
    {
        private readonly IUserService _userService;

        protected override int[] ProcessPasses => new[]
        {
            1,4
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All Umbraco Commerce Stores";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Store;

        public UmbracoCommerceStoreServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor, IUserService userService)
            : base(umbracoCommerceApi, settingsAccessor)
        {
            _userService = userService;
        }

        public override string GetEntityName(StoreReadOnly entity)
            => entity.Name;

        public override Task<StoreReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((StoreReadOnly?)_umbracoCommerceApi.GetStore(id));

        public override IAsyncEnumerable<StoreReadOnly> GetEntitiesAsync(CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetStores().ToAsyncEnumerable();

        public override Task<StoreArtifact?> GetArtifactAsync(GuidUdi? udi, StoreReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<StoreArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection();

            var artifact = new StoreArtifact(udi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                MeasurementSystem = (int)entity.MeasurementSystem,
                PricesIncludeTax = entity.PricesIncludeTax,
                CookieTimeout = entity.CookieTimeout,
                CartNumberTemplate = entity.CartNumberTemplate,
                OrderNumberTemplate = entity.OrderNumberTemplate,
                OrderRoundingMethod = (int)entity.OrderRoundingMethod,
                ProductPropertyAliases = entity.ProductPropertyAliases,
                ProductUniquenessPropertyAliases = entity.ProductUniquenessPropertyAliases,
                GiftCardCodeLength = entity.GiftCardCodeLength,
                GiftCardDaysValid = entity.GiftCardDaysValid,
                GiftCardCodeTemplate = entity.GiftCardCodeTemplate,
                GiftCardPropertyAliases = entity.GiftCardPropertyAliases,
                GiftCardActivationMethod = (int)entity.GiftCardActivationMethod,
                AllowedUsers = entity.AllowedUsers.Select(x => x.UserId).ToList(),
                AllowedUserRoles = entity.AllowedUserRoles.Select(x => x.Role).ToList(),
            };

            // Base currency
            if (entity.BaseCurrencyId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, entity.BaseCurrencyId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.BaseCurrencyUdi = depUdi;
            }

            // Default country
            if (entity.DefaultCountryId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, entity.DefaultCountryId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultCountryUdi = depUdi;
            }

            // Default tax class
            if (entity.DefaultTaxClassId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.TaxClass, entity.DefaultTaxClassId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultTaxClassUdi = depUdi;
            }

            // Default location
            if (entity.DefaultLocationId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Location, entity.DefaultLocationId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultLocationUdi = depUdi;
            }

            // Default order status
            if (entity.DefaultOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.OrderStatus, entity.DefaultOrderStatusId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultOrderStatusUdi = depUdi;
            }

            // Error order status
            if (entity.ErrorOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.OrderStatus, entity.ErrorOrderStatusId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorOrderStatusUdi = depUdi;
            }

            // Gift card activation order status
            if (entity.GiftCardActivationOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.OrderStatus, entity.GiftCardActivationOrderStatusId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.GiftCardActivationOrderStatusUdi = depUdi;
            }

            // Gift card email template
            if (entity.DefaultGiftCardEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, entity.DefaultGiftCardEmailTemplateId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultGiftCardEmailTemplateUdi = depUdi;
            }

            // Confirmation email template
            if (entity.ConfirmationEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, entity.ConfirmationEmailTemplateId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ConfirmationEmailTemplateUdi = depUdi;
            }

            // Error email template
            if (entity.ErrorEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, entity.ErrorEmailTemplateId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorEmailTemplateUdi = depUdi;
            }

            // Stock sharing store
            if (entity.ShareStockFromStoreId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.ShareStockFromStoreId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ShareStockFromStoreUdi = depUdi;
            }

            return Task.FromResult<StoreArtifact?>(artifact);
        }

        public override async Task ProcessAsync(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 1:
                    await Pass1Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                case 4:
                    await Pass4Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private async Task Pass1Async(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default)
            => await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    StoreArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    Store? entity = state.Entity?.AsWritable(uow) ?? Store.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        false);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetMeasurementSystem((MeasurementSystem)artifact.MeasurementSystem)
                        .SetPriceTaxInclusivity(artifact.PricesIncludeTax)
                        .SetCartNumberTemplate(artifact.CartNumberTemplate)
                        .SetOrderNumberTemplate(artifact.OrderNumberTemplate)
                        .SetOrderRoundingMethod((OrderRoundingMethod)artifact.OrderRoundingMethod)
                        .SetProductPropertyAliases(artifact.ProductPropertyAliases, SetBehavior.Replace)
                        .SetProductUniquenessPropertyAliases(artifact.ProductUniquenessPropertyAliases, SetBehavior.Replace)
                        .SetGiftCardCodeLength(artifact.GiftCardCodeLength)
                        .SetGiftCardCodeTemplate(artifact.GiftCardCodeTemplate)
                        .SetGiftCardValidityTimeframe(artifact.GiftCardDaysValid)
                        .SetGiftCardPropertyAliases(artifact.GiftCardPropertyAliases, SetBehavior.Replace)
                        .SetGiftCardActivationMethod((GiftCardActivationMethod)artifact.GiftCardActivationMethod)
                        .SetSortOrder(artifact.SortOrder)
                        .SetAllowedUsers(artifact.AllowedUsers)
                        .SetAllowedUserRoles(artifact.AllowedUserRoles);

                    if (artifact.CookieTimeout.HasValue)
                    {
                        entity.EnableCookies(artifact.CookieTimeout.Value);
                    }
                    else
                    {
                        entity.DisableCookies();
                    }

                    _umbracoCommerceApi.SaveStore(entity);

                    state.Entity = entity;

                    uow.Complete();
                },
                cancellationToken)
                .ConfigureAwait(false);

        private Task Pass4Async(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    StoreArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        Store? entity = _umbracoCommerceApi.GetStore(state.Entity.Id).AsWritable(uow);

                        // BaseCurrency
                        Guid? baseCurrencyId = null;

                        if (artifact.BaseCurrencyUdi != null)
                        {
                            artifact.BaseCurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);

                            baseCurrencyId = _umbracoCommerceApi.GetCurrency(artifact.BaseCurrencyUdi.Guid)?.Id;
                        }

                        entity.SetBaseCurrency(baseCurrencyId);

                        // DefaultCountry
                        Guid? defaultCountryId = null;

                        if (artifact.DefaultCountryUdi != null)
                        {
                            artifact.DefaultCountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                            defaultCountryId = _umbracoCommerceApi.GetCountry(artifact.DefaultCountryUdi.Guid)?.Id;
                        }

                        entity.SetDefaultCountry(defaultCountryId);

                        // DefaultTaxClass
                        Guid? defaultTaxClassId = null;

                        if (artifact.DefaultTaxClassUdi != null)
                        {
                            artifact.DefaultTaxClassUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);

                            defaultTaxClassId = _umbracoCommerceApi.GetTaxClass(artifact.DefaultTaxClassUdi.Guid)?.Id;
                        }

                        entity.SetDefaultTaxClass(defaultTaxClassId);

                        // DefaultLocation
                        Guid? defaultLocationId = null;

                        if (artifact.DefaultLocationUdi != null)
                        {
                            artifact.DefaultLocationUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Location);

                            defaultLocationId = _umbracoCommerceApi.GetLocation(artifact.DefaultLocationUdi.Guid)?.Id;
                        }

                        entity.SetDefaultLocation(defaultLocationId);

                        // DefaultOrderStatus
                        Guid? defaultOrderStatusId = null;

                        if (artifact.DefaultOrderStatusUdi != null)
                        {
                            artifact.DefaultOrderStatusUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);

                            defaultOrderStatusId = _umbracoCommerceApi.GetOrderStatus(artifact.DefaultOrderStatusUdi.Guid)?.Id;
                        }

                        entity.SetDefaultOrderStatus(defaultOrderStatusId);

                        // ErrorOrderStatus
                        Guid? errorOrderStatusId = null;

                        if (artifact.ErrorOrderStatusUdi != null)
                        {
                            artifact.ErrorOrderStatusUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);

                            errorOrderStatusId = _umbracoCommerceApi.GetOrderStatus(artifact.ErrorOrderStatusUdi.Guid)?.Id;
                        }

                        entity.SetErrorOrderStatus(errorOrderStatusId);

                        // DefaultGiftCardEmailTemplate
                        Guid? defaultGiftCardEmailTemplateId = null;

                        if (artifact.DefaultGiftCardEmailTemplateUdi != null)
                        {
                            artifact.DefaultGiftCardEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            defaultGiftCardEmailTemplateId = _umbracoCommerceApi.GetEmailTemplate(artifact.DefaultGiftCardEmailTemplateUdi.Guid)?.Id;
                        }

                        entity.SetDefaultGiftCardEmailTemplate(defaultGiftCardEmailTemplateId);

                        // ConfirmationEmailTemplate
                        Guid? confirmationEmailTemplateId = null;

                        if (artifact.ConfirmationEmailTemplateUdi != null)
                        {
                            artifact.ConfirmationEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            confirmationEmailTemplateId = _umbracoCommerceApi.GetEmailTemplate(artifact.ConfirmationEmailTemplateUdi.Guid)?.Id;
                        }

                        entity.SetConfirmationEmailTemplate(confirmationEmailTemplateId);

                        // ErrorEmailTemplate
                        Guid? errorEmailTemplateId = null;

                        if (artifact.ErrorEmailTemplateUdi != null)
                        {
                            artifact.ErrorEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            errorEmailTemplateId = _umbracoCommerceApi.GetEmailTemplate(artifact.ErrorEmailTemplateUdi.Guid)?.Id;
                        }

                        entity.SetErrorEmailTemplate(errorEmailTemplateId);

                        // StockSharingStore
                        Guid? stockSharingStore = null;

                        if (artifact.ShareStockFromStoreUdi != null)
                        {
                            artifact.ShareStockFromStoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                            stockSharingStore = _umbracoCommerceApi.GetStore(artifact.ShareStockFromStoreUdi.Guid)?.Id;
                        }

                        if (stockSharingStore.HasValue)
                        {
                            entity.ShareStockFrom(stockSharingStore.Value);
                        }
                        else
                        {
                            entity.StopSharingStock();
                        }

                        _umbracoCommerceApi.SaveStore(entity);
                    }

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
