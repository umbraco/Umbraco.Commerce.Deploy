using System.Text.Json;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Commerce.Deploy;

public static class Constants
{
    internal static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        Converters =
        {
            new JsonUdiConverter()
        }
    };
}
