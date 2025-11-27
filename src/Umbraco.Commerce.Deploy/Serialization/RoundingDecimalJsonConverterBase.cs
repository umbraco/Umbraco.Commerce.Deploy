using System;
using System.Text.Json.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Deploy.Infrastructure.Serialization;

/// <summary>
/// Provides base functionality for <see cref="JsonConverter"/>s for rounding decimal values.
/// </summary>
/// <typeparam name="T">The type of object or value handled by the converter.</typeparam>
public abstract class RoundingDecimalJsonConverterBase<T> : WriteOnlyJsonConverter<T>
{
    private readonly int _precision;
    private readonly MidpointRounding _rounding;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingDecimalJsonConverter"/> class using a default precision of 2.
    /// </summary>
    protected RoundingDecimalJsonConverterBase()
        : this(2)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingDecimalJsonConverter"/> class using the provided precision.
    /// </summary>
    /// <param name="precision">Required rounding precision.</param>
    protected RoundingDecimalJsonConverterBase(int precision)
        : this(precision, MidpointRounding.AwayFromZero)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoundingDecimalJsonConverter"/> classusing the provided precision and midpoint rounding
    /// </summary>
    /// <param name="precision">Required rounding precision.</param>
    /// <param name="rounding">Required <see cref="MidpointRounding"/>.</param>
    public RoundingDecimalJsonConverterBase(int precision, MidpointRounding rounding)
    {
        _precision = precision;
        _rounding = rounding;
    }

    /// <summary>
    /// Rounds a decimal value.
    /// </summary>
    protected decimal GetRoundedValue(decimal value) => Math.Round(value, _precision, _rounding);

    /// <summary>
    /// Removes unnecessary trailing zeros from a provided decimal value.
    /// </summary>
    protected decimal RemoveUnnecessaryTrailingZeros(decimal roundedValue)
    {
        for (int i = _precision - 1; i > 0; i--)
        {
            var newRoundedValue = Math.Round(roundedValue, i, _rounding);
            if (newRoundedValue != roundedValue)
            {
                break;
            }

            roundedValue = newRoundedValue;
        }

        return roundedValue;
    }
}
