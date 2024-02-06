using Umbraco.Commerce.Core.Events;
using Umbraco.Commerce.Core.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Disk;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using Umbraco.Extensions;
using Umbraco.Cms.Core;
using System.Linq;

namespace Umbraco.Commerce.Deploy.Composing
{
    public partial class UmbracoCommerceDeployComponent : IComponent
    {
        private readonly IDiskEntityService _diskEntityService;
        private readonly IServiceConnectorFactory _serviceConnectorFactory;
        private readonly ITransferEntityService _transferEntityService;

        public UmbracoCommerceDeployComponent(
            IDiskEntityService diskEntityService,
            IServiceConnectorFactory serviceConnectorFactory,
            ITransferEntityService transferEntityService)
        {
            _diskEntityService = diskEntityService;
            _serviceConnectorFactory = serviceConnectorFactory;
            _transferEntityService = transferEntityService;
        }

        public void Initialize()
        {
            RegisterUdiTypes();
            InitializeDiskRefreshers();
            InitializeIntegratedEntities();
        }

        public void Terminate()
        { }

        private void RegisterUdiTypes()
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
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate, UdiType.GuidUdi);

            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi);
        }

        private void InitializeIntegratedEntities()
        {
            // Add in integrated transfer entities
            _transferEntityService.RegisterTransferEntityType<ProductAttributeReadOnly>(
                UmbracoCommerceConstants.UdiEntityType.ProductAttribute,
                "Product Attributes",
                new DeployRegisteredEntityTypeDetailOptions
                {
                    SupportsQueueForTransfer = true,
                    SupportsQueueForTransferOfDescendents = true,
                    SupportsRestore = true,
                    PermittedToRestore = true,
                    SupportsPartialRestore = true,
                    //SupportsImportExport = true,
                    //SupportsExportOfDescendants = true
                },
                false,
                Cms.Constants.Trees.Stores.Alias,
                (string routePath, HttpContext httpContext) => MatchesRoutePath(routePath, "productattribute"),
                (string nodeId, HttpContext httpContext) => MatchesNodeId(
                    nodeId,
                    httpContext,
                    [
                        Cms.Constants.Trees.Stores.NodeType.ProductAttributes,
                        Cms.Constants.Trees.Stores.NodeType.ProductAttribute
                    ]),
                (string nodeId, HttpContext httpContext, out Guid entityId) =>
                {
                    if (Guid.TryParse(nodeId, out entityId))
                    {
                        return true;
                    }
                    else if (int.TryParse(nodeId, out int id) && id == Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributes])
                    {
                        entityId = Guid.Empty;
                        return true;
                    }
                    else
                    {
                        entityId = Guid.Empty;
                        return false;
                    }
                });
                // TODO: , new DeployTransferRegisteredEntityTypeDetail.RemoteTreeDetail(FormsTreeHelper.GetExampleTree, "example", "externalExampleTree"));

            _transferEntityService.RegisterTransferEntityType<ProductAttributePresetReadOnly>(
                UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset,
                "Product Attribute Presets",
                new DeployRegisteredEntityTypeDetailOptions
                {
                    SupportsQueueForTransfer = true,
                    SupportsQueueForTransferOfDescendents = true,
                    SupportsRestore = true,
                    PermittedToRestore = true,
                    SupportsPartialRestore = true,
                    //SupportsImportExport = true,
                    //SupportsExportOfDescendants = true
                },
                false,
                Cms.Constants.Trees.Stores.Alias,
                (string routePath, HttpContext httpContext) => MatchesRoutePath(routePath, "productattributepreset"),
                (string nodeId, HttpContext httpContext) => MatchesNodeId(
                    nodeId,
                    httpContext,
                    [
                        Cms.Constants.Trees.Stores.NodeType.ProductAttributePresets,
                        Cms.Constants.Trees.Stores.NodeType.ProductAttributePreset
                    ]),
                (string nodeId, HttpContext httpContext, out Guid entityId) =>
                {
                    if (Guid.TryParse(nodeId, out entityId))
                    {
                        return true;
                    }
                    else if (int.TryParse(nodeId, out int id) && id == Cms.Constants.Trees.Stores.Ids[Cms.Constants.Trees.Stores.NodeType.ProductAttributePresets])
                    {
                        entityId = Guid.Empty;
                        return true;
                    }
                    else
                    {
                        entityId = Guid.Empty;
                        return false;
                    }
                });
                // TODO: , new DeployTransferRegisteredEntityTypeDetail.RemoteTreeDetail(FormsTreeHelper.GetExampleTree, "example", "externalExampleTree"));
        }

        private static bool MatchesRoutePath(string routePath, string routePartPrefix)
            => routePath.StartsWith($"commerce/commerce/{routePartPrefix}-");

        private static bool MatchesNodeId(string nodeId, HttpContext httpContext, Cms.Constants.Trees.Stores.NodeType[] nodeTypes)
        {
            if (int.TryParse(nodeId, out int id))
            {
                foreach (var nt in nodeTypes)
                {
                    if (Cms.Constants.Trees.Stores.Ids.ContainsKey(nt) && Cms.Constants.Trees.Stores.Ids[nt] == id)
                    {
                        return true;
                    }
                }
            }

            var nodeType = httpContext.Request.Query["nodeType"].ToString();
            return nodeTypes.Select(x => x.ToString()).InvariantContains(nodeType);
        }

        private void InitializeDiskRefreshers()
        {
            // Add in settings entities as valid Disk entities that can be written out	
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Store);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Location);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Country);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Region);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Currency);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate);
            _diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate);

            // TODO: Other entities

            // Stores
            EventHub.NotificationEvents.OnStoreSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Store) }));
            EventHub.NotificationEvents.OnStoreDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Store) }));

            // Location
            EventHub.NotificationEvents.OnLocationSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Location) }));
            EventHub.NotificationEvents.OnLocationDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Location) }));

            // OrderStatus
            EventHub.NotificationEvents.OnOrderStatusSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));
            EventHub.NotificationEvents.OnOrderStatusDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));

            // ShippingMethod
            EventHub.NotificationEvents.OnShippingMethodSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.ShippingMethod) }));
            EventHub.NotificationEvents.OnShippingMethodDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.ShippingMethod) }));

            // PaymentMethod
            EventHub.NotificationEvents.OnPaymentMethodSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.PaymentMethod) }));
            EventHub.NotificationEvents.OnPaymentMethodDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.PaymentMethod) }));

            // Country
            EventHub.NotificationEvents.OnCountrySaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Country) }));
            EventHub.NotificationEvents.OnCountryDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Country) }));

            // Region
            EventHub.NotificationEvents.OnRegionSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Region) }));
            EventHub.NotificationEvents.OnRegionDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Region) }));

            // Currency
            EventHub.NotificationEvents.OnCurrencySaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Currency) }));
            EventHub.NotificationEvents.OnCurrencyDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Currency) }));

            // TaxClass
            EventHub.NotificationEvents.OnTaxClassSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));
            EventHub.NotificationEvents.OnTaxClassDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));

            // EmailTemplate
            EventHub.NotificationEvents.OnEmailTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));
            EventHub.NotificationEvents.OnEmailTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));

            // PrintTemplate
            EventHub.NotificationEvents.OnPrintTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.PrintTemplate) }));
            EventHub.NotificationEvents.OnPrintTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.PrintTemplate) }));

            // ExportTemplate
            EventHub.NotificationEvents.OnExportTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.ExportTemplate) }));
            EventHub.NotificationEvents.OnExportTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.ExportTemplate) }));

            // TODO: Other entity events
        }

        private IArtifact GetEntityArtifact(EntityBase entity)
        {
            var udi = entity.GetUdi();

            return _serviceConnectorFactory
                .GetConnector(udi.EntityType)
                .GetArtifact(entity, null);
        }
    }
}
