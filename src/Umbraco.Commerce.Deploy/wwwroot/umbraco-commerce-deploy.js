export const onInit = async (host, extensionRegistry) => {
  extensionRegistry.registerMany([
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
    }
  ]);
};
