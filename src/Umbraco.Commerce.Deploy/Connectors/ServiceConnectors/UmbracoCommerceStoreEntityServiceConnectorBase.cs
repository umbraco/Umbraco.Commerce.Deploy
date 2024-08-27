using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    public abstract class UmbracoCommerceStoreEntityServiceConnectorBase<TArtifact, TEntityReadOnly, TEntityWritable,
        TEntityState>(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        : UmbracoCommerceEntityServiceConnectorBase<TArtifact, TEntityReadOnly>(umbracoCommerceApi, settingsAccessor)
        where TArtifact : StoreEntityArtifactBase
        where TEntityReadOnly : StoreAggregateBase<TEntityReadOnly, TEntityWritable, TEntityState>
        where TEntityWritable : TEntityReadOnly
        where TEntityState : StoreAggregateStateBase
    {
        public override async IAsyncEnumerable<TEntityReadOnly> GetEntitiesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IAsyncEnumerable<StoreReadOnly> stores = _umbracoCommerceApi.GetStores()
                .ToAsyncEnumerable();

            await foreach (StoreReadOnly store in stores)
            {
                await foreach (TEntityReadOnly storeEntity in GetEntitiesAsync(store.Id, cancellationToken))
                {
                    yield return storeEntity;
                }
            }
        }

        public abstract IAsyncEnumerable<TEntityReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default);
    }
}
