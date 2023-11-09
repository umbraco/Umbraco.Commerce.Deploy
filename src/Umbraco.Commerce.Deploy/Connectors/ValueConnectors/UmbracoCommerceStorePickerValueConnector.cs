using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
{
    public class UmbracoCommerceStorePickerValueConnector : IValueConnector2
    {
        private readonly IUmbracoCommerceApi _umbracoCommerceApi;
        private readonly UmbracoCommerceDeploySettingsAccessor _settingsAccessor;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.StorePicker" };

        public UmbracoCommerceStorePickerValueConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        {
            _umbracoCommerceApi = umbracoCommerceApi;
            _settingsAccessor = settingsAccessor;
        }

        public string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (!Guid.TryParse(svalue, out var storeId))
                return null;

            var store = _umbracoCommerceApi.GetStore(storeId);
            if (store == null)
                return null;

            var udi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, storeId);

            dependencies.Add(new UmbracoCommerceArtifactDependency(udi));

            return udi.ToString();
        }

        public object FromArtifact(string value, IPropertyType propertyType, object currentValue, IContextCache contextCache)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out var udi) || udi.EntityType != UmbracoCommerceConstants.UdiEntityType.Store)
                return null;

            var store = _umbracoCommerceApi.GetStore(udi.Guid);
            if (store != null)
                return store.Id.ToString();

            return null;
        }
    }
}
