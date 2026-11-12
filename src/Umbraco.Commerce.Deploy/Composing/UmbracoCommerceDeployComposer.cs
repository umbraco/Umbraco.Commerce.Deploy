using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Commerce.Cms;
using Umbraco.Commerce.Core.Data;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;
using Umbraco.Deploy.Infrastructure;
using Umbraco.Deploy.Infrastructure.Transfer;

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

            builder.Services.Configure<DeployEntityTypesOptions>(options =>
            {
                // Settings entities are serialized to disk as UDA files
                options
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Store)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Location)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.OrderStatus)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ShippingMethod)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PaymentMethod)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Country)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Region)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.Currency)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxClass)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate)
                    .AddDiskEntityType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate);

                // Product attributes and presets are content-like, so they're transferred via the backoffice
                options
                    .AddTransferEntityType(
                        UmbracoCommerceConstants.UdiEntityType.ProductAttribute,
                        new DeployTransferEntityTypeOptions
                        {
                            SupportsQueueForTransfer = true,
                            SupportsRestore = true,
                            PermittedToRestore = true,
                            SupportsPartialRestore = true,
                            SupportsImportExport = true,
                        })
                    .AddTransferEntityType(
                        UmbracoCommerceConstants.UdiEntityType.ProductAttributePreset,
                        new DeployTransferEntityTypeOptions
                        {
                            SupportsQueueForTransfer = true,
                            SupportsRestore = true,
                            PermittedToRestore = true,
                            SupportsPartialRestore = true,
                            SupportsImportExport = true,
                        });
            });

            builder.Components()
                .Append<UmbracoCommerceDeployComponent>();
        }
    }
}
