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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi)]
    public class UmbracoCommercePrintTemplateServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<PrintTemplateArtifact, PrintTemplateReadOnly, PrintTemplate, PrintTemplateState>
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Print Templates";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.PrintTemplate;

        public UmbracoCommercePrintTemplateServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(PrintTemplateReadOnly entity)
            => entity.Name;

        public override PrintTemplateReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetPrintTemplate(id);

        public override IEnumerable<PrintTemplateReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetPrintTemplates(storeId);

        public override PrintTemplateArtifact GetArtifact(GuidUdi udi, PrintTemplateReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return new PrintTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? PrintTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetCategory((TemplateCategory)artifact.Category)
                    .SetTemplateView(artifact.TemplateView)
                    .SetSortOrder(artifact.SortOrder);

                _umbracoCommerceApi.SavePrintTemplate(entity);

                uow.Complete();
            });
        }
    }
}
