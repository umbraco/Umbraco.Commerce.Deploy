using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;

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
            IAsyncEnumerable<StoreReadOnly> stores = _umbracoCommerceApi.GetStoresAsync()
                .AsAsyncEnumerable();

            await foreach (StoreReadOnly store in stores)
            {
                await foreach (TEntityReadOnly storeEntity in GetEntitiesAsync(store.Id, cancellationToken))
                {
                    yield return storeEntity;
                }
            }
        }

        public abstract IAsyncEnumerable<TEntityReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a page of tree items, for connectors that implement <see cref="ITreeServiceConnector" />. The entities of all
        /// stores are listed flat at the root, with the store name added when there's more than one store.
        /// </summary>
        /// <param name="parentUdi">The parent UDI: only <c>null</c> or the root of this entity type has items.</param>
        /// <param name="filter">When set, only includes the entities whose name contains it.</param>
        /// <param name="skip">The number of items to skip.</param>
        /// <param name="take">The number of items to take.</param>
        /// <param name="icon">The icon of the tree items.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The tree items and the total number of matching items.</returns>
        protected async Task<PagedModel<TreeItem>> GetStoreEntityTreeItemsAsync(
            Udi? parentUdi,
            string? filter,
            int skip,
            int take,
            string icon,
            CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(skip);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

            // The entities don't have children, and a parent of another entity type isn't ours to list
            if (parentUdi is not null && (!parentUdi.IsRoot || parentUdi.EntityType != UdiEntityType))
            {
                return new PagedModel<TreeItem>();
            }

            var stores = (await _umbracoCommerceApi.GetStoresAsync().ConfigureAwait(false)).ToList();
            var includeStoreName = stores.Count > 1;

            var items = new List<TreeItem>();
            foreach (StoreReadOnly store in stores)
            {
                await foreach (TEntityReadOnly entity in GetEntitiesAsync(store.Id, cancellationToken).ConfigureAwait(false))
                {
                    if (entity.GetUdi() is not GuidUdi udi)
                    {
                        continue;
                    }

                    var name = GetEntityName(entity) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(filter) && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    items.Add(new TreeItem(udi, includeStoreName ? $"{name} ({store.Name})" : name, icon, false, null));
                }
            }

            return new PagedModel<TreeItem>(items.Count, items.Skip(skip).Take(take).ToList());
        }
    }
}
