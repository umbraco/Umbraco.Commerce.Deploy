using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Core.Models;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Disk;

namespace Umbraco.Commerce.Deploy.NotificationHandlers;

/// <summary>
/// Writes the UDA file of a Commerce settings entity when it's saved, and deletes it when the entity is deleted.
/// </summary>
/// <remarks>
/// Commerce raises its own notification events (not CMS notifications), so this is a Commerce event handler instead of a Deploy
/// refresher notification handler. Deploy only writes and deletes the UDA files of enabled disk entity types, so nothing is written
/// when the entity type is excluded. Deploy doesn't cache the signatures of Commerce entity types, so they don't need refreshing.
/// </remarks>
internal sealed class UmbracoCommerceDeployRefresherNotificationHandler(
    IDiskEntityService diskEntityService,
    IServiceConnectorFactory serviceConnectorFactory)
    : IAsyncEventHandlerFor<StoreSavedNotification>,
    IAsyncEventHandlerFor<StoreDeletedNotification>,
    IAsyncEventHandlerFor<LocationSavedNotification>,
    IAsyncEventHandlerFor<LocationDeletedNotification>,
    IAsyncEventHandlerFor<OrderStatusSavedNotification>,
    IAsyncEventHandlerFor<OrderStatusDeletedNotification>,
    IAsyncEventHandlerFor<ShippingMethodSavedNotification>,
    IAsyncEventHandlerFor<ShippingMethodDeletedNotification>,
    IAsyncEventHandlerFor<PaymentMethodSavedNotification>,
    IAsyncEventHandlerFor<PaymentMethodDeletedNotification>,
    IAsyncEventHandlerFor<CountrySavedNotification>,
    IAsyncEventHandlerFor<CountryDeletedNotification>,
    IAsyncEventHandlerFor<RegionSavedNotification>,
    IAsyncEventHandlerFor<RegionDeletedNotification>,
    IAsyncEventHandlerFor<CurrencySavedNotification>,
    IAsyncEventHandlerFor<CurrencyDeletedNotification>,
    IAsyncEventHandlerFor<TaxClassSavedNotification>,
    IAsyncEventHandlerFor<TaxClassDeletedNotification>,
    IAsyncEventHandlerFor<TaxCalculationMethodSavedNotification>,
    IAsyncEventHandlerFor<TaxCalculationMethodDeletedNotification>,
    IAsyncEventHandlerFor<EmailTemplateSavedNotification>,
    IAsyncEventHandlerFor<EmailTemplateDeletedNotification>,
    IAsyncEventHandlerFor<PrintTemplateSavedNotification>,
    IAsyncEventHandlerFor<PrintTemplateDeletedNotification>,
    IAsyncEventHandlerFor<ExportTemplateSavedNotification>,
    IAsyncEventHandlerFor<ExportTemplateDeletedNotification>
{
    /// <inheritdoc />
    public Task HandleAsync(IEvent evt, CancellationToken cancellationToken = default)
        => evt switch
        {
            StoreSavedNotification x => WriteArtifactAsync(x.Store, cancellationToken),
            StoreDeletedNotification x => DeleteArtifact(x.Store),
            LocationSavedNotification x => WriteArtifactAsync(x.Location, cancellationToken),
            LocationDeletedNotification x => DeleteArtifact(x.Location),
            OrderStatusSavedNotification x => WriteArtifactAsync(x.OrderStatus, cancellationToken),
            OrderStatusDeletedNotification x => DeleteArtifact(x.OrderStatus),
            ShippingMethodSavedNotification x => WriteArtifactAsync(x.ShippingMethod, cancellationToken),
            ShippingMethodDeletedNotification x => DeleteArtifact(x.ShippingMethod),
            PaymentMethodSavedNotification x => WriteArtifactAsync(x.PaymentMethod, cancellationToken),
            PaymentMethodDeletedNotification x => DeleteArtifact(x.PaymentMethod),
            CountrySavedNotification x => WriteArtifactAsync(x.Country, cancellationToken),
            CountryDeletedNotification x => DeleteArtifact(x.Country),
            RegionSavedNotification x => WriteArtifactAsync(x.Region, cancellationToken),
            RegionDeletedNotification x => DeleteArtifact(x.Region),
            CurrencySavedNotification x => WriteArtifactAsync(x.Currency, cancellationToken),
            CurrencyDeletedNotification x => DeleteArtifact(x.Currency),
            TaxClassSavedNotification x => WriteArtifactAsync(x.TaxClass, cancellationToken),
            TaxClassDeletedNotification x => DeleteArtifact(x.TaxClass),
            TaxCalculationMethodSavedNotification x => WriteArtifactAsync(x.TaxCalculationMethod, cancellationToken),
            TaxCalculationMethodDeletedNotification x => DeleteArtifact(x.TaxCalculationMethod),
            EmailTemplateSavedNotification x => WriteArtifactAsync(x.EmailTemplate, cancellationToken),
            EmailTemplateDeletedNotification x => DeleteArtifact(x.EmailTemplate),
            PrintTemplateSavedNotification x => WriteArtifactAsync(x.PrintTemplate, cancellationToken),
            PrintTemplateDeletedNotification x => DeleteArtifact(x.PrintTemplate),
            ExportTemplateSavedNotification x => WriteArtifactAsync(x.ExportTemplate, cancellationToken),
            ExportTemplateDeletedNotification x => DeleteArtifact(x.ExportTemplate),
            _ => Task.CompletedTask,
        };

    private async Task WriteArtifactAsync(EntityBase entity, CancellationToken cancellationToken)
    {
        if (entity.GetUdi() is not GuidUdi udi)
        {
            return;
        }

        IArtifact? artifact = await serviceConnectorFactory
            .GetConnector(udi.EntityType)
            .GetArtifactAsync(entity, new DictionaryCache(), cancellationToken)
            .ConfigureAwait(false);

        if (artifact is not null)
        {
            await diskEntityService.WriteArtifactsAsync([artifact], cancellationToken).ConfigureAwait(false);
        }
    }

    private Task DeleteArtifact(EntityBase entity)
    {
        // The entity no longer exists, so delete its UDA file by UDI instead of getting its artifact
        if (entity.GetUdi() is GuidUdi udi)
        {
            diskEntityService.DeleteArtifacts([udi]);
        }

        return Task.CompletedTask;
    }
}
