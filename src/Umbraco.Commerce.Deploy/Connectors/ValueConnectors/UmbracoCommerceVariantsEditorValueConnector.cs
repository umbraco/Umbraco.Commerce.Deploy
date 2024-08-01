// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json.Serialization;
// using Umbraco.Commerce.Core.Api;
// using Umbraco.Cms.Core;
// using Umbraco.Cms.Core.Deploy;
// using Umbraco.Cms.Core.Models;
// using Umbraco.Cms.Core.Services;
// using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using Umbraco.Deploy.Infrastructure.Connectors.ValueConnectors;
// using Umbraco.Cms.Core.Models.Blocks;
// using Umbraco.Deploy.Core.Migrators;
// using Umbraco.Deploy.Infrastructure.Extensions;
//
// namespace Umbraco.Commerce.Deploy.Connectors.ValueConnectors
// {
//     /// <summary>
//     /// A Deploy connector for the Umbraco Commerce Variants Editor property editor
//     /// </summary>
//     public class UmbracoCommerceVariantsEditorValueConnector : BlockValueConnectorBase<UmbracoCommerceVariantsEditorValueConnector.VariantsBlockEditorValue>, IValueConnector
//     {
//         private readonly IUmbracoCommerceApi _umbracoCommerceApi;
//
//         public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.Commerce.VariantsEditor" };
//
//         public UmbracoCommerceVariantsEditorValueConnector(
//             IUmbracoCommerceApi umbracoCommerceApi,
//             IContentTypeService contentTypeService,
//             Lazy<ValueConnectorCollection> valueConnectors,
//             PropertyTypeMigratorCollection propertyTypeMigrators,
//             ILogger<UmbracoCommerceVariantsEditorValueConnector> logger)
//             : base(contentTypeService, valueConnectors, propertyTypeMigrators, logger)
//         {
//             _umbracoCommerceApi = umbracoCommerceApi;
//         }
//
//         public override string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
//         {
//             if (value is string input && input.TryParseJson(out VariantsBlockEditorValue result) && result != null)
//             {
//                 // Recursive
//                 result.Content = ToArtifact(result.Content, dependencies, contextCache).ToList();
//                 result.Settings = ToArtifact(result.Settings, dependencies, contextCache).ToList();
//
//                 var productAttributeAliases = result.Layout.Items.SelectMany(x => x.Config.Attributes.Keys)
//                    .Distinct();
//
//                 foreach (var productAttributeAlias in productAttributeAliases)
//                 {
//                     var productAttribute = _umbracoCommerceApi.GetProductAttribute(result.StoreId.Value, productAttributeAlias);
//                     if (productAttribute != null)
//                     {
//                         dependencies.Add(new UmbracoCommerceArtifactDependency(productAttribute.GetUdi()));
//                     }
//                 }
//
//                 return JsonConvert.SerializeObject(result, Formatting.None);
//             }
//
//             return null;
//         }
//
//         public override object FromArtifact(string value, IPropertyType propertyType, object currentValue, IDictionary<string, string> propertyEditorAliases, IContextCache contextCache)
//         {
//             if (value is string input && input.TryParseJson(out VariantsBlockEditorValue result) && result != null)
//             {
//                 // Recursive
//                 result.Content = FromArtifact(result.Content, propertyEditorAliases, contextCache).ToList();
//                 result.Settings = FromArtifact(result.Settings, propertyEditorAliases, contextCache).ToList();
//
//                 return JsonConvert.SerializeObject(result, Formatting.None);
//             }
//
//             return null;
//         }
//
//         object IValueConnector.FromArtifact(string value, IPropertyType propertyType, object currentValue, IContextCache contextCache)
//             => FromArtifact(value, propertyType, currentValue, contextCache);
//
//         string IValueConnector.ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
//             => ToArtifact(value, propertyType, dependencies, contextCache);
//
//         public class BaseValue
//         {
//             public Guid? StoreId { get; set; }
//         }
//
//         public class VariantsBlockEditorValue : BlockValue
//         {
//             public override string PropertyEditorAlias => "Umbraco.Commerce.VariantsEditor";
//         }
//
//         public class VariantsBlockEditorLayoutItem : IBlockLayoutItem
//         {
//             public Udi? ContentUdi { get; set; }
//             public Udi? SettingsUdi { get; set; }
//             public ProductVariantConfig Config { get; set; }
//         }
//
//         public class ProductVariantConfig
//         {
//             public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
//
//             public bool IsDefault { get; set; }
//         }
//     }
// }
