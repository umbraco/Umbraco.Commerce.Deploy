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
    [UdiDefinition(UmbracoCommerceConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi)]
    public class UmbracoCommerceEmailTemplateServiceConnector : UmbracoCommerceStoreEntityServiceConnectorBase<EmailTemplateArtifact, EmailTemplateReadOnly, EmailTemplate, EmailTemplateState>
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

        public override string AllEntitiesRangeName => "All Umbraco Commerce Email Templates";

        public override string UdiEntityType => UmbracoCommerceConstants.UdiEntityType.EmailTemplate;

        public UmbracoCommerceEmailTemplateServiceConnector(IUmbracoCommerceApi umbracoCommerceApi, UmbracoCommerceDeploySettingsAccessor settingsAccessor)
            : base(umbracoCommerceApi, settingsAccessor)
        { }

        public override string GetEntityName(EmailTemplateReadOnly entity)
            => entity.Name;

        public override EmailTemplateReadOnly GetEntity(Guid id)
            => _umbracoCommerceApi.GetEmailTemplate(id);

        public override IEnumerable<EmailTemplateReadOnly> GetEntities(Guid storeId)
            => _umbracoCommerceApi.GetEmailTemplates(storeId);

        public override EmailTemplateArtifact GetArtifact(GuidUdi udi, EmailTemplateReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(UmbracoCommerceConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new UmbracoCommerceArtifactDependency(storeUdi)
            };

            return new EmailTemplateArtifact(udi, storeUdi, dependencies)
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
            };
        }

        public override void Process(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context)
        {
            _umbracoCommerceApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(UmbracoCommerceConstants.UdiEntityType.EmailTemplate);
                artifact.StoreUdi.EnsureType(UmbracoCommerceConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? EmailTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

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
            });
        }
    }
}
