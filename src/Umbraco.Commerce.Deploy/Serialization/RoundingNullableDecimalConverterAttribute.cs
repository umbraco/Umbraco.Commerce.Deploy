using System;
using System.Text.Json.Serialization;

namespace Umbraco.Deploy.Infrastructure.Serialization;

/// <summary>
/// Defines an attribute for use with <see cref="RoundingNullableDecimalJsonConverter"/> allowing a parameter
/// to be passed to define the precision required.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RoundingNullableDecimalConverterAttribute : JsonConverterAttribute
{
    private readonly int _precision;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingDecimalJsonConverterAttribute"/> class.
    /// </summary>
    /// <param name="precision"></param>
    public RoundingNullableDecimalConverterAttribute(int precision) => _precision = precision;

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (typeToConvert != typeof(decimal?))
        {
            throw new ArgumentException(
                $"This converter only works with nullable decimal, and it was provided {typeToConvert.Name}.");
        }

        return new RoundingNullableDecimalJsonConverter(_precision);
    }
}
