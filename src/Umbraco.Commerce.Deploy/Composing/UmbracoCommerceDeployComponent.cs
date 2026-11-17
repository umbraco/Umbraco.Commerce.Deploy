using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Infrastructure.Disk;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Commerce.Deploy.Composing;

public class UmbracoCommerceDeployComponent(
    IDiskEntityService diskEntityService,
    IServiceConnectorFactory serviceConnectorFactory)
    : IAsyncComponent
{
    public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        RegisterUdiTypes();

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
}
