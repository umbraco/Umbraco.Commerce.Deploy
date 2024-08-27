using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Deploy.Infrastructure.Serialization;

/// <summary>
/// Provides a <see cref="JsonConverter"/> for rounding decimal values.
/// </summary>
/// <remarks>
/// The decimal value provided will be rounded to the precision specified.
/// Unnecessary trailing zeros will be removed (e.g. 1.20 rounded to 2dp will serialize to 1.2).
/// </remarks>
public class RoundingNullableDecimalJsonConverter : RoundingDecimalJsonConverterBase<decimal?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingNullableDecimalJsonConverter"/> class using a default precision of 2.
    /// </summary>
    public RoundingNullableDecimalJsonConverter()
        : base(2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingNullableDecimalJsonConverter"/> class using the provided precision.
    /// </summary>
    /// <param name="precision">Required rounding precision.</param>
    public RoundingNullableDecimalJsonConverter(int precision)
        : base(precision, MidpointRounding.AwayFromZero)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingNullableDecimalJsonConverter"/> classusing the provided precision and midpoint rounding
    /// </summary>
    /// <param name="precision">Required rounding precision.</param>
    /// <param name="rounding">Required <see cref="MidpointRounding"/>.</param>
    public RoundingNullableDecimalJsonConverter(int precision, MidpointRounding rounding)
        : base(precision, rounding)
    {
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        var roundedValue = GetRoundedValue(value.Value);

        roundedValue = RemoveUnnecessaryTrailingZeros(roundedValue);

        JsonSerializer.Serialize(writer, roundedValue, options);
    }
}
