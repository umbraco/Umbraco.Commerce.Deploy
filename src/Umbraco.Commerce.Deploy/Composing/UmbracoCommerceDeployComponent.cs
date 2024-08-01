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
using System.Threading.Tasks;
using Umbraco.Commerce.Deploy.Utils;
using Umbraco.Deploy.Core;

namespace Umbraco.Commerce.Deploy.Composing
{
    public partial class UmbracoCommerceDeployComponent(
        IDiskEntityService diskEntityService,
        IServiceConnectorFactory serviceConnectorFactory,
        ITransferEntityService transferEntityService)
        : IComponent
    {
        public void Initialize()
        {
            RegisterUdiTypes();
            InitializeDiskRefreshers();
            InitializeIntegratedEntities();
        }

        public void Terminate()
        { }

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
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi);
        }

        private void InitializeIntegratedEntities()
        {
            // Add in integrated transfer entities
            transferEntityService.RegisterTransferEntityType<ProductAttributeReadOnly>(
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

            transferEntityService.RegisterTransferEntityType<ProductAttributePresetReadOnly>(
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

        // TODO: This path is wrong
        private static bool MatchesRoutePath(string routePath, string routePartPrefix)
            => routePath.InvariantStartsWith($"commerce/commerce/{routePartPrefix}-");

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
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Store);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Location);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Country);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Region);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Currency);
            diskEntityService.RegisterDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxClass);
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
}
