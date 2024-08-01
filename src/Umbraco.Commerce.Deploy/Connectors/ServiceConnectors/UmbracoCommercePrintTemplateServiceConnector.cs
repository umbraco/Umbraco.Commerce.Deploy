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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi)]
    public class UmbracoCommercePrintTemplateServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<PrintTemplateArtifact, PrintTemplateReadOnly, PrintTemplate, PrintTemplateState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Print Templates";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.PrintTemplate;

        public UmbracoCommercePrintTemplateServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(PrintTemplateReadOnly entity)
            => entity.Name;

        public override Task<PrintTemplateReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((PrintTemplateReadOnly?)_umbracoCommerceApi.GetPrintTemplate(id));

        public override IAsyncEnumerable<PrintTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetPrintTemplates(storeId).ToAsyncEnumerable();

        public override Task<PrintTemplateArtifact?> GetArtifactAsync(GuidUdi? udi, PrintTemplateReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<PrintTemplateArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return Task.FromResult<PrintTemplateArtifact?>(new PrintTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            });
        }

        public override async Task ProcessAsync(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    PrintTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    PrintTemplate? entity = state.Entity?.AsWritable(uow) ?? PrintTemplate.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetCategory((TemplateCategory)artifact.Category)
                        .SetTemplateView(artifact.TemplateView)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SavePrintTemplate(entity);

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
