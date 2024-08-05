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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.ExportTemplate, UdiType.GuidUdi)]
    public class UmbracoCommerceExportTemplateServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<ExportTemplateArtifact, ExportTemplateReadOnly, ExportTemplate, ExportTemplateState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Export Templates";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.ExportTemplate;

        public UmbracoCommerceExportTemplateServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(ExportTemplateReadOnly entity)
            => entity.Name;

        public override Task<ExportTemplateReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult((ExportTemplateReadOnly?)_umbracoCommerceApi.GetExportTemplate(id));

        public override IAsyncEnumerable<ExportTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetExportTemplates(storeId).ToAsyncEnumerable();

        public override Task<ExportTemplateArtifact?> GetArtifactAsync(GuidUdi? udi, ExportTemplateReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<ExportTemplateArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi),
            };

            return Task.FromResult<ExportTemplateArtifact?>(new ExportTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                FileMimeType = entity.FileMimeType,
                FileExtension = entity.FileExtension,
                ExportStrategy = (int)entity.ExportStrategy,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            });
        }

        public override async Task ProcessAsync(ArtifactDeployState<ExportTemplateArtifact, ExportTemplateReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private Task Pass2Async(ArtifactDeployState<ExportTemplateArtifact, ExportTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    ExportTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    ExportTemplate? entity = state.Entity?.AsWritable(uow) ?? ExportTemplate.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetCategory((TemplateCategory)artifact.Category)
                        .SetFileMimeType(artifact.FileMimeType)
                        .SetFileExtension(artifact.FileExtension)
                        .SetExportStrategy((ExportStrategy)artifact.ExportStrategy)
                        .SetTemplateView(artifact.TemplateView)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveExportTemplate(entity);

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
