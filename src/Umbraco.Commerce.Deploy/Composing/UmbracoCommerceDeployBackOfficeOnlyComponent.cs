using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Infrastructure.Transfer;

namespace Umbraco.Commerce.Deploy.Composing;

/// <summary>
/// Registers the Commerce settings entities as transfer entity types, so they can be queued for transfer and exported from the backoffice.
/// </summary>
/// <remarks>
/// Only added when the Deploy edition is BackOfficeOnly, the same as Deploy does for its own schema entities.
/// </remarks>
internal sealed class UmbracoCommerceDeployBackOfficeOnlyComponent(ITransferEntityService transferEntityService)
    : IAsyncComponent
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

    public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        foreach (var entityType in SettingsEntityTypes)
        {
            transferEntityService.RegisterTransferEntityType(entityType, new DeployRegisteredEntityTypeDetailOptions
            {
                SupportsQueueForTransfer = true,
                SupportsImportExport = true,
            });
        }

        return Task.CompletedTask;
    }

    public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
