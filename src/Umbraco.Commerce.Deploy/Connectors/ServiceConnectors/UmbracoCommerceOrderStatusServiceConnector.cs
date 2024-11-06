using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.OrderStatus, UdiType.GuidUdi)]
    public class UmbracoCommerceOrderStatusServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<OrderStatusArtifact, OrderStatusReadOnly, OrderStatus, OrderStatusState>
    {
        protected override int[] ProcessPasses => new[]
        {
            2
        };

        protected override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        protected override string OpenUdiName => "All Umbraco Commerce Order Statuses";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.OrderStatus;

        public UmbracoCommerceOrderStatusServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(OrderStatusReadOnly entity)
            => entity.Name;

        public override Task<OrderStatusReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetOrderStatusAsync(id);

        public override IAsyncEnumerable<OrderStatusReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetOrderStatusesAsync(storeId).AsAsyncEnumerable();

        public override Task<OrderStatusArtifact?> GetArtifactAsync(GuidUdi? udi, OrderStatusReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<OrderStatusArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return Task.FromResult<OrderStatusArtifact?>(new OrderStatusArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Color = entity.Color,
                SortOrder = entity.SortOrder
            });
        }

        public override async Task ProcessAsync(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            switch (pass)
            {
                case 2:
                    await Pass2Async(state, context, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private async Task Pass2Async(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    OrderStatusArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    OrderStatus? entity = await state.Entity?.AsWritableAsync(uow)! ?? await OrderStatus.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetColorAsync(artifact.Color)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveOrderStatusAsync(entity, ct);

                    await uow.CompleteAsync();
                },
                cancellationToken);
    }
}
