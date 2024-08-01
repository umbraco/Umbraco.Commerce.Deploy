using Umbraco.Commerce.Core.Models;
using Umbraco.Cms.Core;

namespace Umbraco.Commerce.Deploy
{
    internal static class UmbracoCommerceUdiGetterExtensions
    {
        public static GuidUdi? GetUdi(this EntityBase entity) =>
            entity switch
            {
                StoreReadOnly store => store.GetUdi(),
                CountryReadOnly country => country.GetUdi(),
                RegionReadOnly region => region.GetUdi(),
                LocationReadOnly location => location.GetUdi(),
                OrderStatusReadOnly orderStatus => orderStatus.GetUdi(),
                CurrencyReadOnly currency => currency.GetUdi(),
                ShippingMethodReadOnly shippingMethod => shippingMethod.GetUdi(),
                PaymentMethodReadOnly paymentMethod => paymentMethod.GetUdi(),
                TaxClassReadOnly taxClass => taxClass.GetUdi(),
                EmailTemplateReadOnly emailTemplate => emailTemplate.GetUdi(),
                PrintTemplateReadOnly printTemplate => printTemplate.GetUdi(),
                ExportTemplateReadOnly exportTemplate => exportTemplate.GetUdi(),
                DiscountReadOnly discount => discount.GetUdi(),
                GiftCardReadOnly giftCard => giftCard.GetUdi(),
                ProductAttributeReadOnly productAtrtibtue => productAtrtibtue.GetUdi(),
                ProductAttributePresetReadOnly productAtrtibtuePreset => productAtrtibtuePreset.GetUdi(),
                _ => null
            };

        public static GuidUdi GetUdi(this StoreReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.Id);

        public static GuidUdi GetUdi(this CountryReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Country, entity.Id);

        public static GuidUdi GetUdi(this RegionReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Region, entity.Id);

        public static GuidUdi GetUdi(this LocationReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Location, entity.Id);

        public static GuidUdi GetUdi(this OrderStatusReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.OrderStatus, entity.Id);

        public static GuidUdi GetUdi(this CurrencyReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Currency, entity.Id);

        public static GuidUdi GetUdi(this ShippingMethodReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, entity.Id);

        public static GuidUdi GetUdi(this PaymentMethodReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.PaymentMethod, entity.Id);

        public static GuidUdi GetUdi(this TaxClassReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.TaxClass, entity.Id);

        public static GuidUdi GetUdi(this EmailTemplateReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, entity.Id);

        public static GuidUdi GetUdi(this PrintTemplateReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, entity.Id);

        public static GuidUdi GetUdi(this ExportTemplateReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ExportTemplate, entity.Id);

        public static GuidUdi GetUdi(this DiscountReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Discount, entity.Id);

        public static GuidUdi GetUdi(this GiftCardReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.GiftCard, entity.Id);

        public static GuidUdi GetUdi(this ProductAttributeReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, entity.Id);

        public static GuidUdi GetUdi(this ProductAttributePresetReadOnly entity)
            => new GuidUdi(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, entity.Id);
    }
}
