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
                // SupportsQueueForTransferOfDescendents = false,
                SupportsRestore = true,
                PermittedToRestore = true,
                SupportsPartialRestore = true,
                SupportsImportExport = true,
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

        // Writing and deleting the UDA files is done by UmbracoCommerceDeployRefresherNotificationHandler
    }
}
