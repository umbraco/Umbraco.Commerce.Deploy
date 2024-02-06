using System;

namespace Umbraco.Commerce.Deploy.Configuration
{
    public class UmbracoCommerceDeploySettings
    {
        public UmbracoCommerceDeployPaymentMethodSettings PaymentMethods { get; set; }
        public UmbracoCommerceDeployShippingMethodSettings ShippingMethods { get; set; }

        public UmbracoCommerceDeploySettings()
        {
            PaymentMethods = new UmbracoCommerceDeployPaymentMethodSettings();
            ShippingMethods = new UmbracoCommerceDeployShippingMethodSettings();
        }
    }

    public class UmbracoCommerceDeployPaymentMethodSettings
    {
        public string[] IgnoreSettings { get; set; }

        public UmbracoCommerceDeployPaymentMethodSettings()
        {
            IgnoreSettings = Array.Empty<string>();
        }
    }

    public class UmbracoCommerceDeployShippingMethodSettings
    {
        public string[] IgnoreSettings { get; set; }

        public UmbracoCommerceDeployShippingMethodSettings()
        {
            IgnoreSettings = Array.Empty<string>();
        }
    }
}
