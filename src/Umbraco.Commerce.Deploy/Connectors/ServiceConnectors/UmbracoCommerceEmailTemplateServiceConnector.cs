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
            => Task.FromResult((EmailTemplateReadOnly?)_umbracoCommerceApi.GetEmailTemplate(id));

        public override IAsyncEnumerable<EmailTemplateReadOnly> GetEntitiesAsync(Guid storeId, CancellationToken cancellationToken = default)
            => _umbracoCommerceApi.GetEmailTemplates(storeId).ToAsyncEnumerable();

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

        private Task Pass2Async(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context, CancellationToken cancellationToken = default) =>
            _umbracoCommerceApi.Uow.ExecuteAsync(
                (uow, ct) =>
                {
                    EmailTemplateArtifact artifact = state.Artifact;

                    artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);
                    artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                    EmailTemplate? entity = state.Entity?.AsWritable(uow) ?? EmailTemplate.Create(
                        uow,
                        artifact.Udi.Guid,
                        artifact.StoreUdi.Guid,
                        artifact.Alias,
                        artifact.Name);

                    entity.SetName(artifact.Name, artifact.Alias)
                        .SetCategory((TemplateCategory)artifact.Category)
                        .SetSendToCustomer(artifact.SendToCustomer)
                        .SetSubject(artifact.Subject)
                        .SetSender(artifact.SenderName, artifact.SenderAddress)
                        .SetToAddresses(artifact.ToAddresses)
                        .SetCcAddresses(artifact.CcAddresses)
                        .SetBccAddresses(artifact.BccAddresses)
                        .SetTemplateView(artifact.TemplateView)
                        .SetSortOrder(artifact.SortOrder);

                    _umbracoCommerceApi.SaveEmailTemplate(entity);

                    uow.Complete();

                    return Task.CompletedTask;
                },
                cancellationToken);
    }
}
