using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public override int[] ProcessPasses => new[]
        {
            1,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Umbraco Commerce Stores";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Store;

        public UmbracoCommerceStoreServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor, IUserService userService)
            : base(umbracoCommerceApi, settingsAccessor)
        {
            _userService = userService;
        }

        public override string GetEntityName(StoreReadOnly entity)
            => entity.Name;

        public override StoreReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetStore(id);

        public override IEnumerable<StoreReadOnly> GetEntities()
            => _umbracoCommerceApi.GetStores();

        public override StoreArtifact GetArtifact(GuidUdi udi, StoreReadOnly entity)
        {
            if (entity == null)
                return null;

            var dependencies = new ArtifactDependencyCollection();

#pragma warning disable CS0618 // OrderEditorConfig is obsolete
            var artifact = new StoreArtifact(udi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                MeasurementSystem = entity.MeasurementSystem,
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
                GiftCardActivationMethod = (int)entity.GiftCardActivationMethod
            };
#pragma warning restore CS0618 // OrderEditorConfig is obsolete

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

            // Allowed users
            // NB: Users can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUsers.Count > 0)
            {
                var users = new List<string>();

                foreach (var id in entity.AllowedUsers)
                {
                    var user = _userService.GetByProviderKey(id.UserId);
                    if (user != null)
                    {
                        users.Add(user.Username);
                    }
                }

                if (users.Count > 0)
                {
                    artifact.AllowedUsers = users;
                }
            }

            // Allowed user roles
            // NB: Users roles can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUserRoles.Count > 0)
            {
                var userRoles = new List<string>();

                foreach (var role in entity.AllowedUserRoles)
                {
                    var userGroup = _userService.GetUserGroupByAlias(role.Role);
                    if (userGroup != null)
                    {
                        userRoles.Add(userGroup.Alias);
                    }
                }

                if (userRoles.Count > 0)
                {
                    artifact.AllowedUserRoles = userRoles;
                }
            }

            // Stock sharing store
            if (entity.ShareStockFromStoreId.HasValue)
            {
                var depUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.ShareStockFromStoreId.Value);
                var dep = new UmbracoCommerceArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ShareStockFromStoreUdi = depUdi;
            }

            return artifact;
        }

        public override void Process(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 1:
                    Pass1(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass1(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow)
                    ?? Store.Create(uow, artifact.Udi.Guid, artifact.Alias, artifact.Name, false);

#pragma warning disable CS0618 // SetOrderEditorConfig is obsolete
                entity.SetName(artifact.Name, artifact.Alias)
                    .SetMeasurementSystem(artifact.MeasurementSystem)
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
                    .SetSortOrder(artifact.SortOrder);
#pragma warning restore CS0618 // SetOrderEditorConfig is obsolete

                if (artifact.CookieTimeout.HasValue)
                {
                    entity.EnableCookies(artifact.CookieTimeout.Value);
                }
                else
                {
                    entity.DisableCookies();
                }

                if (artifact.AllowedUsers != null && artifact.AllowedUsers.Any())
                {
                    var userIds = artifact.AllowedUsers.Select(x => _userService.GetByUsername(x))
                        .Where(x => x != null)
                        .Select(x => x.Id.ToString(CultureInfo.InvariantCulture))
                        .ToList();

                    entity.SetAllowedUsers(userIds, SetBehavior.Replace);
                }

                if (artifact.AllowedUserRoles != null && artifact.AllowedUserRoles.Any())
                {
                    var userRoles = artifact.AllowedUserRoles.Select(x => _userService.GetUserGroupByAlias(x))
                        .Where(x => x != null)
                        .Select(x => x.Alias)
                        .ToList();

                    entity.SetAllowedUserRoles(userRoles, SetBehavior.Replace);
                }

                _umbracoCommerceApi.SaveStore(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _umbracoCommerceApi.GetStore(state.Entity.Id).AsWritable(uow);

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
                    entity.ShareStockFrom(stockSharingStore.Value);
                else
                    entity.StopSharingStock();

                _umbracoCommerceApi.SaveStore(entity);

                uow.Complete();
            });
        }
    }
}
