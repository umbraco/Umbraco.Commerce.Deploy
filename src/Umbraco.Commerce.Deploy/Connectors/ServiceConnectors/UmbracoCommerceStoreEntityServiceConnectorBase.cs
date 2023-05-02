using System;
using System.Collections.Generic;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    public abstract class UmbracoCommerceStoreEntityServiceConnectorBase<TArtifact, TEntityReadOnly, TEntityWritable, TEntityState> : UmbracoCommerceEntityServiceConnectorBase<TArtifact, TEntityReadOnly>
        where TArtifact : StoreEntityArtifactBase
        where TEntityReadOnly : StoreAggregateBase<TEntityReadOnly, TEntityWritable, TEntityState>
        where TEntityWritable : TEntityReadOnly
        where TEntityState : StoreAggregateStateBase
    {
        public UmbracoCommerceStoreEntityServiceConnectorBase(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override IEnumerable<TEntityReadOnly> GetEntities()
        {
            var stores = _umbracoCommerceApi.GetStores();
            var storeEntities = new List<TEntityReadOnly>();

            foreach (var store in stores)
            {
                storeEntities.AddRange(GetEntities(store.Id));
            }

            return storeEntities;
        }

        public abstract IEnumerable<TEntityReadOnly> GetEntities(Guid storeId);
    }
}
