using System;
using Umbraco.Commerce.Core.Events;
using Umbraco.Commerce.Core.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Disk;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Transfer;
using Umbraco.Cms.Core;
using System.Threading;
using System.Threading.Tasks;
using J2N.Text;
using Umbraco.Deploy.Core;

namespace Umbraco.Commerce.Deploy.Composing;

public class UmbracoCommerceDeployComponent(
    IDiskEntityService diskEntityService,
    IServiceConnectorFactory serviceConnectorFactory,
    ITransferEntityService transferEntityService)
    : IAsyncComponent
{
    public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        RegisterUdiTypes();
        InitializeDiskRefreshers();
        InitializeIntegratedEntities();

        return Task.CompletedTask;
    }

    public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    private static void RegisterUdiTypes()
    {
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.Store, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.Location, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.OrderStatus, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.Country, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.Region, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.Currency, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.TaxClass, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi);
        UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi);
    }

    private void InitializeIntegratedEntities()
    {
        // Add in integrated transfer entities
        transferEntityService.RegisterTransferEntityType(
            UmbracoCommerceConstants.UdiEntityType.ProductAttribute,
            new DeployRegisteredEntityTypeDetailOptions
            {
                SupportsQueueForTransfer = true,
                //SupportsQueueForTransferOfDescendents = true,
                SupportsRestore = true,
                PermittedToRestore = true,
                SupportsPartialRestore = true,
                SupportsImportExport = true,
            },
            (string id, bool descendants, IServiceProvider provider, out UdiRange? range) =>
            {
                // Root folder
                if (int.TryParse(id, out var numericId) && numericId == Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributes])
                {
                    range = new UdiRange(Udi.Create(UmbracoCommerceConstants.UdiEntityType.ProductAttribute), Constants.DeploySelector.DescendantsOfThis);
                    return true;
                }

                // Single entity
                if (Guid.TryParse(id, out Guid guidId))
                {
                    range = new UdiRange(Udi.Create(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, guidId));
                    return true;
                }

                // Out of range
                range = null;
                return false;
            });

        transferEntityService.RegisterTransferEntityType(
            UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset,
            new DeployRegisteredEntityTypeDetailOptions
            {
                SupportsQueueForTransfer = true,
                //SupportsQueueForTransferOfDescendents = true,
                SupportsRestore = true,
                PermittedToRestore = true,
                SupportsPartialRestore = true,
                SupportsImportExport = true,
            },
            (string id, bool descendants, IServiceProvider provider, out UdiRange? range) =>
            {
                // Root folder
                if (int.TryParse(id, out var numericId) && numericId == Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributePresets])
                {
                    range = new UdiRange(Udi.Create(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset), Constants.DeploySelector.DescendantsOfThis);
                    return true;
                }

                // Single entity
                if (Guid.TryParse(id, out Guid guidId))
                {
                    range = new UdiRange(Udi.Create(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, guidId));
                    return true;
                }

                // Out of range
                range = null;
                return false;
            });
    }

    private void InitializeDiskRefreshers()
    {
        // Add in settings entities as valid Disk entities that can be written out
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Store);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Location);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Country);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Region);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Currency);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate);
        diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate);

        // TODO: Other entities

        // Stores
        EventHub.NotificationEvents.OnStoreSaved((e) => WriteEntityArtifact(e.Store));
        EventHub.NotificationEvents.OnStoreDeleted((e) => DeleteEntityArtifact(e.Store));

        // Location
        EventHub.NotificationEvents.OnLocationSaved((e) => WriteEntityArtifact(e.Location));
        EventHub.NotificationEvents.OnLocationDeleted((e) => DeleteEntityArtifact(e.Location));

        // OrderStatus
        EventHub.NotificationEvents.OnOrderStatusSaved((e) => WriteEntityArtifact(e.OrderStatus));
        EventHub.NotificationEvents.OnOrderStatusDeleted((e) => DeleteEntityArtifact(e.OrderStatus));

        // ShippingMethod
        EventHub.NotificationEvents.OnShippingMethodSaved((e) => WriteEntityArtifact(e.ShippingMethod));
        EventHub.NotificationEvents.OnShippingMethodDeleted((e) => DeleteEntityArtifact(e.ShippingMethod));

        // PaymentMethod
        EventHub.NotificationEvents.OnPaymentMethodSaved((e) => WriteEntityArtifact(e.PaymentMethod));
        EventHub.NotificationEvents.OnPaymentMethodDeleted((e) => DeleteEntityArtifact(e.PaymentMethod));

        // Country
        EventHub.NotificationEvents.OnCountrySaved((e) => WriteEntityArtifact(e.Country));
        EventHub.NotificationEvents.OnCountryDeleted((e) => DeleteEntityArtifact(e.Country));

        // Region
        EventHub.NotificationEvents.OnRegionSaved((e) => WriteEntityArtifact(e.Region));
        EventHub.NotificationEvents.OnRegionDeleted((e) => DeleteEntityArtifact(e.Region));

        // Currency
        EventHub.NotificationEvents.OnCurrencySaved((e) => WriteEntityArtifact(e.Currency));
        EventHub.NotificationEvents.OnCurrencyDeleted((e) => DeleteEntityArtifact(e.Currency));

        // TaxClass
        EventHub.NotificationEvents.OnTaxClassSaved((e) => WriteEntityArtifact(e.TaxClass));
        EventHub.NotificationEvents.OnTaxClassDeleted((e) => DeleteEntityArtifact(e.TaxClass));

        // TaxCalculationMethod
        EventHub.NotificationEvents.OnTaxCalculationMethodSaved((e) => WriteEntityArtifact(e.TaxCalculationMethod));
        EventHub.NotificationEvents.OnTaxCalculationMethodDeleted((e) => DeleteEntityArtifact(e.TaxCalculationMethod));

        // EmailTemplate
        EventHub.NotificationEvents.OnEmailTemplateSaved((e) => WriteEntityArtifact(e.EmailTemplate));
        EventHub.NotificationEvents.OnEmailTemplateDeleted((e) => DeleteEntityArtifact(e.EmailTemplate));

        // PrintTemplate
        EventHub.NotificationEvents.OnPrintTemplateSaved((e) => WriteEntityArtifact(e.PrintTemplate));
        EventHub.NotificationEvents.OnPrintTemplateDeleted((e) => DeleteEntityArtifact(e.PrintTemplate));

        // ExportTemplate
        EventHub.NotificationEvents.OnExportTemplateSaved((e) => WriteEntityArtifact(e.ExportTemplate));
        EventHub.NotificationEvents.OnExportTemplateDeleted((e) => DeleteEntityArtifact(e.ExportTemplate));

        // TODO: Other entity events
    }

    private void WriteEntityArtifact(EntityBase entity) =>
        AsyncHelper.RunSync(async () =>
        {
            IArtifact? artifact = await GetEntityArtifactAsync(entity).ConfigureAwait(false);
            if (artifact != null)
            {
                await diskEntityService.WriteArtifactsAsync(new[] { artifact! }).ConfigureAwait(false);
            }
        });

    private void DeleteEntityArtifact(EntityBase entity) =>
        AsyncHelper.RunSync(async () =>
        {
            IArtifact? artifact = await GetEntityArtifactAsync(entity).ConfigureAwait(false);
            if (artifact != null)
            {
                diskEntityService.DeleteArtifacts(new[] { artifact! });
            }
        });

    private async Task<IArtifact?> GetEntityArtifactAsync(EntityBase entity)
    {
        GuidUdi? udi = entity.GetUdi();

        if (udi == null)
        {
            return null;
        }

        return await serviceConnectorFactory
            .GetConnector(udi.EntityType)
            .GetArtifactAsync(entity, new DictionaryCache())
            .ConfigureAwait(false);
    }
}
