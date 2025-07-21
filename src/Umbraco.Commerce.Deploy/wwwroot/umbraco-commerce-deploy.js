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
            "umbraco-commerce-product-attribute": "Umbraco Commerce Product Attribute",
            "umbraco-commerce-product-attribute-preset": "Umbraco Commerce Product Attribute Preset"
          }
        }
      },
    },
    {
      type: "deployEntityActionRegistrar",
      actionAlias: "Deploy.EntityAction.Queue",
      alias: "Uc.Deploy.Queue.Registrar",
      name: "Umbraco Commerce Deploy Queue Entity Action Registrar",
      forEntityTypes: [
        "uc:product-attribute",
        "uc:product-attributes",
        "uc:product-attribute-preset",
        "uc:product-attribute-presets"
      ],
    },
    {
      type: "deployEntityTypeMapping",
      alias: "Uc.Deploy.EntityTypeMapping",
      name: "Umbraco Commerce Deploy Entity Type Mapping",
      entityTypes: {
        "uc:product-attribute": "umbraco-commerce-product-attribute",
        "uc:product-attributes": "umbraco-commerce-product-attribute",
        "uc:product-attribute-preset": "umbraco-commerce-product-attribute-preset",
        "uc:product-attribute-presets": "umbraco-commerce-product-attribute-preset"
      },
    },
  ]);
};
