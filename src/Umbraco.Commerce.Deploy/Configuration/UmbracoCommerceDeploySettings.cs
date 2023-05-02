using System;

namespace Umbraco.Commerce.Deploy.Configuration
{
    public class UmbracoCommerceDeploySettings
    {
        public UmbracoCommerceDeployPaymentMethodSettings PaymentMethods { get; set; }

        public UmbracoCommerceDeploySettings()
        {
            PaymentMethods = new UmbracoCommerceDeployPaymentMethodSettings();
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
}
