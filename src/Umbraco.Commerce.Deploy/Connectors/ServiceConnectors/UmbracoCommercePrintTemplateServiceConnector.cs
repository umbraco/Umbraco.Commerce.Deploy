using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Artifacts;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Commerce.Extensions;

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
            => _umbracoCommerceApi.GetPrintTemplateAsync(id);

        public override IAsyncEnumerable<PrintTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetPrintTemplatesAsync(storeId).AsAsyncEnumerable();

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

        private async Task Pass2Async(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    PrintTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.PrintTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    PrintTemplate? entity = await state.Entity?.AsWritableAsync(uow)! ?? await PrintTemplate.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetCategoryAsync((TemplateCategory)artifact.Category)
                        .SetTemplateViewAsync(artifact.TemplateView)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SavePrintTemplateAsync(entity, ct);

                    await uow.CompleteAsync();
                },
                cancellationToken);
    }
}
