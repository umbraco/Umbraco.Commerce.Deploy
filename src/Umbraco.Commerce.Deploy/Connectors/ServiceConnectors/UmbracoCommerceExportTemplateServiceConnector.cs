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
            => _umbracoCommerceApi.GetExportTemplateAsync(id);

        public override IAsyncEnumerable<ExportTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetExportTemplatesAsync(storeId).AsAsyncEnumerable();

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

        private async Task Pass2Async(ArtifactDeployState<ExportTemplateArtifact, ExportTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    ExportTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.ExportTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    ExportTemplate? entity = state.Entity != null ? await state.Entity.AsWritableAsync(uow) : await ExportTemplate.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetCategoryAsync((TemplateCategory)artifact.Category)
                        .SetFileMimeTypeAsync(artifact.FileMimeType)
                        .SetFileExtensionAsync(artifact.FileExtension)
                        .SetExportStrategyAsync((ExportStrategy)artifact.ExportStrategy)
                        .SetTemplateViewAsync(artifact.TemplateView)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveExportTemplateAsync(entity, ct);

                    uow.Complete();
                },
                cancellationToken);
    }
}
