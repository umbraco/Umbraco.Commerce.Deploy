using System;
using System.Collections.Generic;
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
        public override int[] ProcessPasses => new[]
        {
            2
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Umbraco Commerce Order Statuses";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.OrderStatus;

        public UmbracoCommerceOrderStatusServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(OrderStatusReadOnly entity)
            => entity.Name;

        public override OrderStatusReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetOrderStatus(id);

        public override IEnumerable<OrderStatusReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetOrderStatuses(storeId);

        public override OrderStatusArtifact GetArtifact(GuidUdi udi, OrderStatusReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return new OrderStatusArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Color = entity.Color,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 2:
                    Pass2(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass2(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.OrderStatus);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? OrderStatus.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetColor(artifact.Color)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveOrderStatus(entity);

                uow.Complete();
            });
        }
    }
}
