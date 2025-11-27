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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi)]
    public class UmbracoCommerceEmailTemplateServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<EmailTemplateArtifact, EmailTemplateReadOnly, EmailTemplate, EmailTemplateState>
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

        protected override string OpenUdiName => "All Umbraco Commerce Email Templates";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.EmailTemplate;

        public UmbracoCommerceEmailTemplateServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(EmailTemplateReadOnly entity)
            => entity.Name;

        public override Task<EmailTemplateReadOnly?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetEmailTemplateAsync(id);

        public override IAsyncEnumerable<EmailTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetEmailTemplatesAsync(storeId).AsAsyncEnumerable();

        public override Task<EmailTemplateArtifact?> GetArtifactAsync(GuidUdi? udi, EmailTemplateReadOnly? entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                return Task.FromResult<EmailTemplateArtifact?>(null);
            }

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi),
            };

            return Task.FromResult<EmailTemplateArtifact?>(new EmailTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                Subject = entity.Subject,
                SenderName = entity.SenderName,
                SenderAddress = entity.SenderAddress,
                ToAddresses = entity.ToAddresses,
                CcAddresses = entity.CcAddresses,
                BccAddresses = entity.BccAddresses,
                SendToCustomer = entity.SendToCustomer,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            });
        }

        public override async Task ProcessAsync(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
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

        private async Task Pass2Async(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            await _umbracoCommerceApi.Uow.ExecuteAsync(
                async (uow, ct) =>
                {
                    EmailTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    EmailTemplate? entity = state.Entity != null ? await state.Entity.AsWritableAsync(uow) : await EmailTemplate.CreateAsync(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    await entity.SetNameAsync(artifact.Name, artifact.Alias)
                        .SetCategoryAsync((TemplateCategory)artifact.Category)
                        .SetSendToCustomerAsync(artifact.SendToCustomer)
                        .SetSubjectAsync(artifact.Subject)
                        .SetSenderAsync(artifact.SenderName, artifact.SenderAddress)
                        .SetToAddressesAsync(artifact.ToAddresses)
                        .SetCcAddressesAsync(artifact.CcAddresses)
                        .SetBccAddressesAsync(artifact.BccAddresses)
                        .SetTemplateViewAsync(artifact.TemplateView)
                        .SetSortOrderAsync(artifact.SortOrder);

                    await _umbracoCommerceApi.SaveEmailTemplateAsync(entity, ct);

                    uow.Complete();
                },
                cancellationToken);
    }
}
