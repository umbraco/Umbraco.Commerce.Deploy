using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Commerce.Cms.PropertyEditors.StorePicker;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommerceStoreEntityPickerValueConnector(
        IDataTypeService dataTypeService,
        IUmbracoCommerceApi umbracoCommerceApi)
        : ValueConnectorBase
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.StoreEntityPicker" };

        public override async Task<string?> ToArtifactAsync(
            object? value,
            IPropertyType propertyType,
            ICollection<ArtifactDependency> dependencies,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                return null;
            }

            if (!Guid.TryParse(svalue, out Guid entityId))
            {
                return null;
            }

            var entityType = await GetPropertyEntityTypeAsync(propertyType, cancellationToken).ConfigureAwait(false);
            if (entityType == null)
            {
                return null;
            }

            EntityBase? entity = await GetEntityAsync(entityType, entityId, cancellationToken).ConfigureAwait(false);

            if (entity == null)
            {
                return null;
            }

            var udi = new GuidUdi(entityType, entity.Id);

            dependencies.Add(new UmbracoCommerceArtifactDependency(udi));

            return udi.ToString();
        }

        public override async Task<object?> FromArtifactAsync(
            string? value,
            IPropertyType propertyType,
            object? currentValue,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out GuidUdi? udi))
            {
                return null;
            }

            EntityBase? entity = await GetEntityAsync(udi!.EntityType, udi.Guid, cancellationToken).ConfigureAwait(false);

            return entity != null ? entity.Id.ToString() : null;
        }

        private async Task<string?> GetPropertyEntityTypeAsync(IPropertyType propertyType, CancellationToken cancellationToken = default)
        {
            IDataType? dataType = await dataTypeService.GetAsync(propertyType.DataTypeKey).ConfigureAwait(false);

            if (dataType != null)
            {
                StoreEntityPickerConfiguration? cfg = dataType.ConfigurationAs<StoreEntityPickerConfiguration>();

                switch (cfg?.EntityType)
                {
                    case "Location":
                        return UmbracoCommerceConstants.UdiEntityType.Location;
                    case "OrderStatus":
                        return UmbracoCommerceConstants.UdiEntityType.OrderStatus;
                    case "Country":
                        return UmbracoCommerceConstants.UdiEntityType.Country;
                    case "ShippingMethod":
                        return UmbracoCommerceConstants.UdiEntityType.ShippingMethod;
                    case "PaymentMethod":
                        return UmbracoCommerceConstants.UdiEntityType.PaymentMethod;
                    case "Currency":
                        return UmbracoCommerceConstants.UdiEntityType.Currency;
                    case "TaxClass":
                        return UmbracoCommerceConstants.UdiEntityType.TaxClass;
                    case "EmailTemplate":
                        return UmbracoCommerceConstants.UdiEntityType.EmailTemplate;
                    case "ExportTemplate":
                        return UmbracoCommerceConstants.UdiEntityType.ExportTemplate;
                    case "PrintTemplate":
                        return UmbracoCommerceConstants.UdiEntityType.PrintTemplate;
                    case "Discount": // Not sure if discounts should transfer as these are "user generated"
                        return UmbracoCommerceConstants.UdiEntityType.Discount;
                }
            }

            return null;
        }

        private Task<EntityBase?> GetEntityAsync(string entityType, Guid id, CancellationToken cancellationToken = default)
        {
            switch (entityType)
            {
                case UmbracoCommerceConstants.UdiEntityType.Location:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetLocation(id));
                case UmbracoCommerceConstants.UdiEntityType.OrderStatus:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetOrderStatus(id));
                case UmbracoCommerceConstants.UdiEntityType.Country:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetCountry(id));
                case UmbracoCommerceConstants.UdiEntityType.ShippingMethod:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetShippingMethod(id));
                case UmbracoCommerceConstants.UdiEntityType.PaymentMethod:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetPaymentMethod(id));
                case UmbracoCommerceConstants.UdiEntityType.Currency:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetCurrency(id));
                case UmbracoCommerceConstants.UdiEntityType.TaxClass:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetTaxClass(id));
                case UmbracoCommerceConstants.UdiEntityType.EmailTemplate:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetEmailTemplate(id));
                case UmbracoCommerceConstants.UdiEntityType.ExportTemplate:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetExportTemplate(id));
                case UmbracoCommerceConstants.UdiEntityType.PrintTemplate:
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetPrintTemplate(id));
                case UmbracoCommerceConstants.UdiEntityType.Discount:  // Not sure if discounts should transfer as these are "user generated"
                    return Task.FromResult<EntityBase?>(umbracoCommerceApi.GetDiscount(id));
            }

            return Task.FromResult<EntityBase?>(null);
        }
    }
}
