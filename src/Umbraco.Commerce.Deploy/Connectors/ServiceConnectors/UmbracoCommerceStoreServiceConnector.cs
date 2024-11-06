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
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Extensions;

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
            => _umbracoCommerceApi.GetStoreAsync(id);

        public override IAsyncEnumerable<StoreReadOnly> GetEntitiesAsync(CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetStoresAsync().AsAsyncEnumerable();

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

                    Store? entity = await state.Entity?.AsWritableAsync(uow)! ?? await Store.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.Alias,
                        artifact.Name,
                        false);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetMeasurementSystemAsync((MeasurementSystem)artifact.MeasurementSystem)
                        .SetPriceTaxInclusivityAsync(artifact.PricesIncludeTax)
                        .SetCartNumberTemplateAsync(artifact.CartNumberTemplate)
                        .SetOrderNumberTemplateAsync(artifact.OrderNumberTemplate)
                        .SetOrderRoundingMethodAsync((OrderRoundingMethod)artifact.OrderRoundingMethod)
                        .SetProductPropertyAliasesAsync(artifact.ProductPropertyAliases, SetBehavior.Replace)
                        .SetProductUniquenessPropertyAliasesAsync(artifact.ProductUniquenessPropertyAliases, SetBehavior.Replace)
                        .SetGiftCardCodeLengthAsync(artifact.GiftCardCodeLength)
                        .SetGiftCardCodeTemplateAsync(artifact.GiftCardCodeTemplate)
                        .SetGiftCardValidityTimeframeAsync(artifact.GiftCardDaysValid)
                        .SetGiftCardPropertyAliasesAsync(artifact.GiftCardPropertyAliases, SetBehavior.Replace)
                        .SetGiftCardActivationMethodAsync((GiftCardActivationMethod)artifact.GiftCardActivationMethod)
                        .SetSortOrderAsync(artifact.SortOrder)
                        .SetAllowedUsersAsync(artifact.AllowedUsers)
                        .SetAllowedUserRolesAsync(artifact.AllowedUserRoles);

                    if (artifact.CookieTimeout.HasValue)
                    {
                        await entity.EnableCookiesAsync(artifact.CookieTimeout.Value);
                    }
                    else
                    {
                        await entity.DisableCookiesAsync();
                    }

                    await _umbracoCommerceApi.SaveStoreAsync(entity, ct);

                    state.Entity = entity;

                    await uow.CompleteAsync();
                },
                cancellationToken);

        private async Task Pass4Async(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    StoreArtifact artifact = state.Artifact;

                    if (state.Entity != null)
                    {
                        Store? entity = await _umbracoCommerceApi.GetStoreAsync(state.Entity.Id).AsWritableAsync(uow);

                        // BaseCurrency
                        Guid? baseCurrencyId = null;

                        if (artifact.BaseCurrencyUdi != null)
                        {
                            artifact.BaseCurrencyUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Currency);

                            baseCurrencyId = (await _umbracoCommerceApi.GetCurrencyAsync(artifact.BaseCurrencyUdi.Guid))?.Id;
                        }

                        await entity.SetBaseCurrencyAsync(baseCurrencyId);

                        // DefaultCountry
                        Guid? defaultCountryId = null;

                        if (artifact.DefaultCountryUdi != null)
                        {
                            artifact.DefaultCountryUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Country);

                            defaultCountryId = (await _umbracoCommerceApi.GetCountryAsync(artifact.DefaultCountryUdi.Guid))?.Id;
                        }

                        await entity.SetDefaultCountryAsync(defaultCountryId);

                        // DefaultTaxClass
                        Guid? defaultTaxClassId = null;

                        if (artifact.DefaultTaxClassUdi != null)
                        {
                            artifact.DefaultTaxClassUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.TaxClass);

                            defaultTaxClassId = (await _umbracoCommerceApi.GetTaxClassAsync(artifact.DefaultTaxClassUdi.Guid))?.Id;
                        }

                        await entity.SetDefaultTaxClassAsync(defaultTaxClassId);

                        // DefaultLocation
                        Guid? defaultLocationId = null;

                        if (artifact.DefaultLocationUdi != null)
                        {
                            artifact.DefaultLocationUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Location);

                            defaultLocationId = (await _umbracoCommerceApi.GetLocationAsync(artifact.DefaultLocationUdi.Guid))?.Id;
                        }

                        await entity.SetDefaultLocationAsync(defaultLocationId);

                        // DefaultOrderStatus
                        Guid? defaultOrderStatusId = null;

                        if (artifact.DefaultOrderStatusUdi != null)
                        {
                            artifact.DefaultOrderStatusUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);

                            defaultOrderStatusId = (await _umbracoCommerceApi.GetOrderStatusAsync(artifact.DefaultOrderStatusUdi.Guid))?.Id;
                        }

                        await entity.SetDefaultOrderStatusAsync(defaultOrderStatusId);

                        // ErrorOrderStatus
                        Guid? errorOrderStatusId = null;

                        if (artifact.ErrorOrderStatusUdi != null)
                        {
                            artifact.ErrorOrderStatusUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);

                            errorOrderStatusId = (await _umbracoCommerceApi.GetOrderStatusAsync(artifact.ErrorOrderStatusUdi.Guid))?.Id;
                        }

                        await entity.SetErrorOrderStatusAsync(errorOrderStatusId);

                        // DefaultGiftCardEmailTemplate
                        Guid? defaultGiftCardEmailTemplateId = null;

                        if (artifact.DefaultGiftCardEmailTemplateUdi != null)
                        {
                            artifact.DefaultGiftCardEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            defaultGiftCardEmailTemplateId = (await _umbracoCommerceApi.GetEmailTemplateAsync(artifact.DefaultGiftCardEmailTemplateUdi.Guid))?.Id;
                        }

                        await entity.SetDefaultGiftCardEmailTemplateAsync(defaultGiftCardEmailTemplateId);

                        // ConfirmationEmailTemplate
                        Guid? confirmationEmailTemplateId = null;

                        if (artifact.ConfirmationEmailTemplateUdi != null)
                        {
                            artifact.ConfirmationEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            confirmationEmailTemplateId = (await _umbracoCommerceApi.GetEmailTemplateAsync(artifact.ConfirmationEmailTemplateUdi.Guid))?.Id;
                        }

                        await entity.SetConfirmationEmailTemplateAsync(confirmationEmailTemplateId);

                        // ErrorEmailTemplate
                        Guid? errorEmailTemplateId = null;

                        if (artifact.ErrorEmailTemplateUdi != null)
                        {
                            artifact.ErrorEmailTemplateUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);

                            errorEmailTemplateId = (await _umbracoCommerceApi.GetEmailTemplateAsync(artifact.ErrorEmailTemplateUdi.Guid))?.Id;
                        }

                        await entity.SetErrorEmailTemplateAsync(errorEmailTemplateId);

                        // StockSharingStore
                        Guid? stockSharingStore = null;

                        if (artifact.ShareStockFromStoreUdi != null)
                        {
                            artifact.ShareStockFromStoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                            stockSharingStore = (await _umbracoCommerceApi.GetStoreAsync(artifact.ShareStockFromStoreUdi.Guid))?.Id;
                        }

                        if (stockSharingStore.HasValue)
                        {
                            await entity.ShareStockFromAsync(stockSharingStore.Value);
                        }
                        else
                        {
                            await entity.StopSharingStockAsync();
                        }

                        await _umbracoCommerceApi.SaveStoreAsync(entity, ct);
                    }

                    await uow.CompleteAsync();
                },
                cancellationToken);
    }
}
