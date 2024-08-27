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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Location, UdiType.GuidUdi)]
    public class UmbracoCommerceLocationServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<LocationArtifact, LocationReadOnly, Location, LocationState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Locations";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Location;

        public UmbracoCommerceLocationServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(LocationReadOnly entity)
            => entity.Name;

        public override Task<LocationReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((LocationReadOnly?)_umbracoCommerceApi.GetLocation(id));

        public override IAsyncEnumerable<LocationReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetLocations(storeId).ToAsyncEnumerable();

        public override Task<LocationArtifact?> GetArtifactAsync(GuidUdi? udi, LocationReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<LocationArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return Task.FromResult<LocationArtifact?>(new LocationArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                City = entity.City,
                Region = entity.Region,
                CountryIsoCode = entity.CountryIsoCode,
                ZipCode = entity.ZipCode,
                Type = (int)entity.Type,
                SortOrder = entity.SortOrder
            });
        }

        public override async Task ProcessAsync(ArtifactDeployState<LocationArtifact, LocationReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<LocationArtifact, LocationReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    LocationArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Location);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    Location? entity = state.Entity?.AsWritable(uow) ?? Location.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetType((LocationType)artifact.Type)
                        .SetAddress(new Address(artifact.AddressLine1, artifact.AddressLine2, artifact.City, artifact.Region, artifact.CountryIsoCode, artifact.ZipCode))
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveLocation(entity);

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
