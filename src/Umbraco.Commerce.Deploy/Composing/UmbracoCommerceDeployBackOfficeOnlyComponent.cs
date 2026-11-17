using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Core.Configuration;
using Umbraco.Deploy.Core.Configuration.DeployConfiguration;
using Umbraco.Deploy.Infrastructure.Transfer;

namespace Umbraco.Commerce.Deploy.Composing;

/// <summary>
/// Registers the Commerce settings entities as transfer entity types when the Deploy edition is BackOfficeOnly, so they can be
/// queued for transfer and exported from the backoffice.
/// </summary>
/// <remarks>
/// Deploy does the same for its own schema entities, because the BackOfficeOnly edition has no git-based deployment of the UDA files.
/// </remarks>
internal sealed class UmbracoCommerceDeployBackOfficeOnlyComponent(
    ITransferEntityService transferEntityService,
    IOptions<DeploySettings> deploySettings)
    : AsyncComponentBase
{
    private static readonly string[] SettingsEntityTypes =
    [
        UmbracoCommerceConstants.UdiEntityType.Store,
        UmbracoCommerceConstants.UdiEntityType.Location,
        UmbracoCommerceConstants.UdiEntityType.OrderStatus,
        UmbracoCommerceConstants.UdiEntityType.ShippingMethod,
        UmbracoCommerceConstants.UdiEntityType.PaymentMethod,
        UmbracoCommerceConstants.UdiEntityType.Country,
        UmbracoCommerceConstants.UdiEntityType.Region,
        UmbracoCommerceConstants.UdiEntityType.Currency,
        UmbracoCommerceConstants.UdiEntityType.TaxClass,
        UmbracoCommerceConstants.UdiEntityType.TaxCalculationMethod,
        UmbracoCommerceConstants.UdiEntityType.EmailTemplate,
        UmbracoCommerceConstants.UdiEntityType.PrintTemplate,
        UmbracoCommerceConstants.UdiEntityType.ExportTemplate,
    ];

    protected override Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (deploySettings.Value.Edition is Edition.BackOfficeOnly)
        {
            foreach (var entityType in SettingsEntityTypes)
            {
                transferEntityService.RegisterTransferEntityType(entityType, new DeployRegisteredEntityTypeDetailOptions
                {
                    SupportsQueueForTransfer = true,
                    SupportsImportExport = true,
                });
            }
        }

        return Task.CompletedTask;
    }
}
