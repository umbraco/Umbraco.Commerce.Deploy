using System;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Cms.PropertyEditors.StorePicker;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommerceStoreEntityPickerValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IUmbracoCommerceApi _umbracoCommerceApi;
        private readonly UmbracoCommerceDeploySettingsAccessor _settingsAccessor;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.StoreEntityPicker" };

        public UmbracoCommerceStoreEntityPickerValueConnector(IDataTypeService dataTypeService,
            IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        {
            _dataTypeService = dataTypeService;
            _umbracoCommerceApi = umbracoCommerceApi;
            _settingsAccessor = settingsAccessor;
        }

        public string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (!Guid.TryParse(svalue, out var entityId))
                return null;

            var entityType = GetPropertyEntityType(propertyType);
            if (entityType == null)
                return null;

            var entity = GetEntity(entityType, entityId);
            if (entity == null)
                return null;

            var udi = new GuidUdi(entityType, entity.Id);

            dependencies.Add(new UmbracoCommerceArtifactDependency(udi));

            return udi.ToString();
        }

        public object FromArtifact(string value, IPropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out var udi))
                return null;

            var entity = GetEntity(udi.EntityType, udi.Guid);
            if (entity != null)
                return entity.Id.ToString();

            return null;
        }

        private string GetPropertyEntityType(IPropertyType propertyType)
        {
            var dataType = _dataTypeService.GetDataType(propertyType.DataTypeId);

            var cfg = dataType.ConfigurationAs<StoreEntityPickerConfiguration>();
            switch (cfg.EntityType)
            {
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
                case "Discount": // Not sure if discounts should transfer as these are "user generated"
                    return UmbracoCommerceConstants.UdiEntityType.Discount;
            }

            return null;
        }

        private EntityBase GetEntity(string entityType, Guid id)
        {
            switch (entityType)
            {
                case UmbracoCommerceConstants.UdiEntityType.OrderStatus:
                    return _umbracoCommerceApi.GetOrderStatus(id);
                case UmbracoCommerceConstants.UdiEntityType.Country:
                    return _umbracoCommerceApi.GetCountry(id);
                case UmbracoCommerceConstants.UdiEntityType.ShippingMethod:
                    return _umbracoCommerceApi.GetShippingMethod(id);
                case UmbracoCommerceConstants.UdiEntityType.PaymentMethod:
                    return _umbracoCommerceApi.GetPaymentMethod(id);
                case UmbracoCommerceConstants.UdiEntityType.Currency:
                    return _umbracoCommerceApi.GetCurrency(id);
                case UmbracoCommerceConstants.UdiEntityType.TaxClass:
                    return _umbracoCommerceApi.GetTaxClass(id);
                case UmbracoCommerceConstants.UdiEntityType.EmailTemplate:
                    return _umbracoCommerceApi.GetEmailTemplate(id);
                case UmbracoCommerceConstants.UdiEntityType.Discount:  // Not sure if discounts should transfer as these are "user generated"
                    return _umbracoCommerceApi.GetDiscount(id);
            }

            return null;
        }
    }
}
