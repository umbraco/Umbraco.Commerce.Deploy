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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.Location, UdiType.GuidUdi)]
    public class UmbracoCommerceLocationServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<LocationArtifact, LocationReadOnly, Location, LocationState>
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Locations";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.Location;

        public UmbracoCommerceLocationServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(LocationReadOnly entity)
            => entity.Name;

        public override LocationReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetLocation(id);

        public override IEnumerable<LocationReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetLocations(storeId);

        public override LocationArtifact GetArtifact(GuidUdi udi, LocationReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return new LocationArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                City = entity.City,
                Region = entity.Region,
                CountryIsoCode = entity.CountryIsoCode,
                ZipCode = entity.ZipCode,
                Type = entity.Type,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<LocationArtifact, LocationReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<LocationArtifact, LocationReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Location);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? Location.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetType(artifact.Type)
                    .SetAddress(new Address(artifact.AddressLine1, artifact.AddressLine2, artifact.City, artifact.Region, artifact.CountryIsoCode, artifact.ZipCode))
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SaveLocation(entity);

                uow.Complete();
            });
        }
    }
}
