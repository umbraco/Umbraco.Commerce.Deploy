using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Cms;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Data;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Deploy.NotificationHandlers;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Deploy.Composing
{
    [ComposeAfter(typeof(UmbracoCommerceComposer))]
    public class UmbracoCommerceDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<UmbracoCommerceDeploySettings>()
                .Bind(builder.Config.GetSection("Umbraco:Commerce:Deploy"));

            // Ensure SQLite support for local development
            builder.WithUmbracoCommerceBuilder().AddSQLite();

            builder.Services.AddUnique<IConfigureOptions<ConnectionStringConfig>, ConfigureUmbracoCommerceDeployConnectionString>();

            builder.Services.AddSingleton<UmbracoCommerceDeploySettingsAccessor>();

            builder.Components()
                .Append<UmbracoCommerceDeployComponent>()
                .Append<UmbracoCommerceDeployBackOfficeOnlyComponent>();

            // Write and delete the UDA files of the settings entities
            IUmbracoCommerceBuilder commerceBuilder = builder.WithUmbracoCommerceBuilder();
            commerceBuilder.WithNotificationEvent<StoreSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<StoreDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<LocationSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<LocationDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<OrderStatusSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<OrderStatusDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<ShippingMethodSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<ShippingMethodDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<PaymentMethodSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<PaymentMethodDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<CountrySavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<CountryDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<RegionSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<RegionDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<CurrencySavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<CurrencyDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<TaxClassSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<TaxClassDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<TaxCalculationMethodSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<TaxCalculationMethodDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<EmailTemplateSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<EmailTemplateDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<PrintTemplateSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<PrintTemplateDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<ExportTemplateSavedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
            commerceBuilder.WithNotificationEvent<ExportTemplateDeletedNotification>().RegisterHandler<UmbracoCommerceDeployRefresherNotificationHandler>();
        }
    }
}
