using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

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
            => Task.FromResult((OrderStatusReadOnly?)_umbracoCommerceApi.GetOrderStatus(id));

        public override IAsyncEnumerable<OrderStatusReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetOrderStatuses(storeId).ToAsyncEnumerable();

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

        private Task Pass2Async(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    OrderStatusArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    OrderStatus? entity = state.Entity?.AsWritable(uow) ?? OrderStatus.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetColor(artifact.Color)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveOrderStatus(entity);

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
