using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;
using DeploySelector = Umbraco.Cms.Core.Constants.DeploySelector;

namespace Umbraco.Commerce.Deploy.Connectors.ServiceConnectors
{
    public abstract class UmbracoCommerceEntityServiceConnectorBase<TArtifact, TEntity>(
        IUmbracoCommerceApi umbracoCommerceApi,
        UmbracoCommerceDeploySettingsAccessor settingsAccessor)
        : ServiceConnectorBase<TArtifact, GuidUdi, TEntity>
        where TArtifact : DeployArtifactBase<GuidUdi>
        where TEntity : EntityBase
    {
        private static readonly string[] EntitySelectors =
        [
            DeploySelector.This,
            DeploySelector.ThisAndChildren,
            DeploySelector.ThisAndDescendants,
            DeploySelector.ChildrenOfThis,
            DeploySelector.DescendantsOfThis,
        ];

        protected readonly IUmbracoCommerceApi _umbracoCommerceApi = umbracoCommerceApi;
        protected readonly UmbracoCommerceDeploySettingsAccessor _settingsAccessor = settingsAccessor;

        public abstract string UdiEntityType { get; }

        public virtual string ContainerId => "-1";

        public abstract string GetEntityName(TEntity entity);

        public abstract Task<TEntity?> GetEntityAsync(Guid id,  CancellationToken cancellationToken = default);

        public abstract IAsyncEnumerable<TEntity> GetEntitiesAsync(CancellationToken cancellationToken = default);

        public abstract Task<TArtifact?> GetArtifactAsync(GuidUdi? udi, TEntity? entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an existing entity by its identifying properties from the artifact (e.g., alias or code).
        /// Used as a fallback when the GUID lookup fails during deployment.
        /// </summary>
        /// <param name="artifact">The artifact containing identifying properties.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The existing entity if found; otherwise, null.</returns>
        public abstract Task<TEntity?> GetExistingEntityAsync(TArtifact artifact, CancellationToken cancellationToken = default);

        public override Task<TArtifact> GetArtifactAsync(
            TEntity entity,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
            => GetArtifactAsync(entity.GetUdi(), entity, cancellationToken)!;

        public override async Task<TArtifact?> GetArtifactAsync(
            GuidUdi? udi,
            IContextCache contextCache,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);
            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);
            return entity == null ? null : await GetArtifactAsync(udi, entity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            GuidUdi udi,
            string selector,
            CancellationToken cancellationToken = default)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(udi, selector);
                return new NamedUdiRange(udi, OpenUdiName, selector);
            }

            EnsureEntitySelector(selector);

            TEntity? entity = await GetEntityAsync(udi.Guid, cancellationToken).ConfigureAwait(false);

            if (entity == null)
            {
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));
            }

            return GetRange(entity, selector);
        }

        public override async Task<NamedUdiRange> GetRangeAsync(
            string entityType,
            string sid,
            string selector,
            CancellationToken cancellationToken = default)
        {
            // Deploy sends an empty GUID for the root, older clients send -1 or the tree container ID
            if (sid == "-1" || sid == ContainerId || (Guid.TryParse(sid, out Guid emptyId) && emptyId == Guid.Empty))
            {
                return GetOpenRange(selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
            {
                throw new ArgumentException("Invalid identifier.", nameof(sid));
            }

            // Check the entity first, because for the store entity type a store ID is the entity itself
            TEntity? entity = await GetEntityAsync(result, cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                EnsureEntitySelector(selector);
                return GetRange(entity, selector);
            }

            // The backoffice collection nodes (e.g. the product attributes of a store) use the store ID, which is the root
            StoreReadOnly? store = await _umbracoCommerceApi.GetStoreAsync(result);
            if (store != null)
            {
                return GetOpenRange(selector);
            }

            throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));
        }

        private NamedUdiRange GetOpenRange(string selector)
        {
            EnsureOpenSelector(selector);
            return new NamedUdiRange(Udi.Create(UdiEntityType), OpenUdiName, selector);
        }

        private NamedUdiRange GetRange(TEntity e, string selector)
            => new(e.GetUdi(), GetEntityName(e), selector);

        /// <summary>
        /// Ensures the selector is valid for an entity (not the root): the entities are flat, so the selectors for children or
        /// descendants are valid, but empty.
        /// </summary>
        /// <param name="selector">The selector.</param>
        /// <exception cref="NotSupportedException">The selector isn't valid for an entity.</exception>
        private static void EnsureEntitySelector(string selector)
        {
            if (!EntitySelectors.Contains(selector))
            {
                throw new NotSupportedException($"Unexpected selector \"{selector}\".");
            }
        }

        private static bool SelectorIncludesEntity(string selector)
            => selector is DeploySelector.This or DeploySelector.ThisAndChildren or DeploySelector.ThisAndDescendants;

        public override async IAsyncEnumerable<GuidUdi?> ExpandRangeAsync(
            UdiRange range,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureType(range.Udi);

            if (range.Udi.IsRoot)
            {
                EnsureSelector(range.Udi, range.Selector);

                await foreach (TEntity entity in GetEntitiesAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return entity.GetUdi();
                }
            }
            else
            {
                EnsureEntitySelector(range.Selector);

                // The entities don't have children, so only the selectors that include the entity itself return it
                if (!SelectorIncludesEntity(range.Selector))
                {
                    yield break;
                }

                TEntity? entity = await GetEntityAsync(((GuidUdi)range.Udi).Guid, cancellationToken).ConfigureAwait(false);

                if (entity == null)
                {
                    yield break;
                }

                yield return entity.GetUdi();
            }
        }

        public override async Task<ArtifactDeployState<TArtifact, TEntity>> ProcessInitAsync(
            TArtifact artifact,
            IDeployContext context,
            CancellationToken cancellationToken = default)
        {
            EnsureType(artifact.Udi);

            // First try to find by GUID
            TEntity? entity = await GetEntityAsync(artifact.Udi.Guid, cancellationToken).ConfigureAwait(false);

            // Fallback: try to find by alias/code if GUID lookup failed (handles GUID mismatches during upgrades)
            entity ??= await GetExistingEntityAsync(artifact, cancellationToken).ConfigureAwait(false);

            return ArtifactDeployState.Create(artifact, entity, this, ProcessPasses[0]);
        }
    }
}
