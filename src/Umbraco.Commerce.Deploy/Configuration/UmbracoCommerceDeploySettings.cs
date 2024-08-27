namespace Umbraco.Commerce.Deploy.Configuration
{
    public class UmbracoCommerceDeploySettings
    {
        public UmbracoCommerceDeployPaymentMethodSettings PaymentMethods { get; set; } = new();
        public UmbracoCommerceDeployShippingMethodSettings ShippingMethods { get; set; } = new();
    }

    public class UmbracoCommerceDeployPaymentMethodSettings
    {
        public string[] IgnoreSettings { get; set; } = [];
    }

    public class UmbracoCommerceDeployShippingMethodSettings
    {
        public string[] IgnoreSettings { get; set; } = [];
    }
}
