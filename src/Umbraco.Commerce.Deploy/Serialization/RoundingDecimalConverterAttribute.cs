using System;
using System.Text.Json.Serialization;

namespace Umbraco.Deploy.Infrastructure.Serialization;

/// <summary>
/// Defines an attribute for use with <see cref="RoundingDecimalJsonConverter"/> allowing a parameter
/// to be passed to define the precision required.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RoundingDecimalConverterAttribute : JsonConverterAttribute
{
    private readonly int _precision;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingDecimalConverterAttribute"/> class.
    /// </summary>
    /// <param name="precision"></param>
    public RoundingDecimalConverterAttribute(int precision) => _precision = precision;

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        if (typeToConvert != typeof(decimal))
        {
            throw new ArgumentException(
                $"This converter only works with decimal, and it was provided {typeToConvert.Name}.");
        }

        return new RoundingDecimalJsonConverter(_precision);
    }
}
