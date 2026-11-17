// Product attributes and presets are transfer entity types: they're always queued, restored and exported via the backoffice.
// Items and collections (whose unique is the store ID, which the connectors treat as the root).
const productAttributeEntityTypes = {
  "uc:product-attribute": "umbraco-commerce-product-attribute",
  "uc:product-attributes": "umbraco-commerce-product-attribute",
  "uc:product-attribute-preset": "umbraco-commerce-product-attribute-preset",
  "uc:product-attribute-presets": "umbraco-commerce-product-attribute-preset",
};

// The settings are disk entity types (UDA files), unless the Deploy edition is BackOfficeOnly, which makes them transfer
// entity types that can be queued and exported. Deploy's own conditions hide the actions when they aren't transfer entity types.
const settingsEntityTypes = {
  // The sidebar root (without unique) and a store (whose unique is the store ID)
  "uc:stores-settings": "umbraco-commerce-store",
  "uc:store-settings": "umbraco-commerce-store",
  "uc:locations": "umbraco-commerce-location",
  "uc:location": "umbraco-commerce-location",
  "uc:order-statuses": "umbraco-commerce-order-status",
  "uc:order-status": "umbraco-commerce-order-status",
  "uc:shipping-methods": "umbraco-commerce-shipping-method",
  "uc:shipping-method": "umbraco-commerce-shipping-method",
  "uc:payment-methods": "umbraco-commerce-payment-method",
  "uc:payment-method": "umbraco-commerce-payment-method",
  "uc:countries": "umbraco-commerce-country",
  "uc:country": "umbraco-commerce-country",
  // Only the item: the regions collection is keyed by its country, which isn't a range of regions
  "uc:region": "umbraco-commerce-region",
  "uc:currencies": "umbraco-commerce-currency",
  "uc:currency": "umbraco-commerce-currency",
  "uc:tax-classes": "umbraco-commerce-tax-class",
  "uc:tax-class": "umbraco-commerce-tax-class",
  "uc:tax-calculation-methods": "umbraco-commerce-tax-calculation-method",
  "uc:tax-calculation-method": "umbraco-commerce-tax-calculation-method",
  "uc:email-templates": "umbraco-commerce-email-template",
  "uc:email-template": "umbraco-commerce-email-template",
  "uc:print-templates": "umbraco-commerce-print-template",
  "uc:print-template": "umbraco-commerce-print-template",
  "uc:export-templates": "umbraco-commerce-export-template",
  "uc:export-template": "umbraco-commerce-export-template",
};

const allEntityTypes = { ...productAttributeEntityTypes, ...settingsEntityTypes };

export const onInit = async (host, extensionRegistry) => {
  extensionRegistry.registerMany([
    {
      type: 'localization',
      alias: 'Uc.Deploy.Localization.En',
      weight: -100,
      name: 'English',
      meta: {
        culture: 'en',
        localizations: {
          // Entity types
          deploy_entityTypes: {
            "umbraco-commerce-store": "Umbraco Commerce Store",
            "umbraco-commerce-location": "Umbraco Commerce Location",
            "umbraco-commerce-order-status": "Umbraco Commerce Order Status",
            "umbraco-commerce-shipping-method": "Umbraco Commerce Shipping Method",
            "umbraco-commerce-payment-method": "Umbraco Commerce Payment Method",
            "umbraco-commerce-country": "Umbraco Commerce Country",
            "umbraco-commerce-region": "Umbraco Commerce Region",
            "umbraco-commerce-currency": "Umbraco Commerce Currency",
            "umbraco-commerce-tax-class": "Umbraco Commerce Tax Class",
            "umbraco-commerce-tax-calculation-method": "Umbraco Commerce Tax Calculation Method",
            "umbraco-commerce-email-template": "Umbraco Commerce Email Template",
            "umbraco-commerce-print-template": "Umbraco Commerce Print Template",
            "umbraco-commerce-export-template": "Umbraco Commerce Export Template",
            "umbraco-commerce-product-attribute": "Umbraco Commerce Product Attribute",
            "umbraco-commerce-product-attribute-preset": "Umbraco Commerce Product Attribute Preset"
          }
        }
      },
    },
    {
      // Shown when the entity type supports queue for transfer
      type: "deployEntityActionRegistrar",
      actionAlias: "Deploy.EntityAction.Queue",
      alias: "Uc.Deploy.Queue.Registrar",
      name: "Umbraco Commerce Deploy Queue Entity Action Registrar",
      forEntityTypes: Object.keys(allEntityTypes),
    },
    {
      // Shown when the entity type supports import and export
      type: "deployEntityActionRegistrar",
      actionAlias: "Deploy.EntityAction.Export",
      alias: "Uc.Deploy.Export.Registrar",
      name: "Umbraco Commerce Deploy Export Entity Action Registrar",
      forEntityTypes: Object.keys(allEntityTypes),
    },
    {
      // Shown when the entity type supports restore: restores all items of the entity type, so only on the collections
      type: "deployEntityActionRegistrar",
      actionAlias: "Deploy.EntityAction.TreeRestore",
      alias: "Uc.Deploy.TreeRestore.Registrar",
      name: "Umbraco Commerce Deploy Tree Restore Entity Action Registrar",
      forEntityTypes: [
        "uc:product-attributes",
        "uc:product-attribute-presets"
      ],
    },
    {
      // Shown when the entity type supports partial restore: the item is preselected and can be changed using the remote tree.
      // Not on the collections, because their unique (the store ID) would be preselected as an item.
      type: "deployEntityActionRegistrar",
      actionAlias: "Deploy.EntityAction.PartialRestore",
      alias: "Uc.Deploy.PartialRestore.Registrar",
      name: "Umbraco Commerce Deploy Partial Restore Entity Action Registrar",
      forEntityTypes: [
        "uc:product-attribute",
        "uc:product-attribute-preset"
      ],
    },
    {
      type: "deployEntityTypeMapping",
      alias: "Uc.Deploy.EntityTypeMapping",
      name: "Umbraco Commerce Deploy Entity Type Mapping",
      entityTypes: allEntityTypes,
    },
  ]);
};
