using Avalonia.Data.Converters;

namespace PleasantUI.Converters;

/// <summary>
/// Provides static methods and properties for converting numeric values.
/// </summary>
public static class NumberConverters
{
    public static readonly IValueConverter DoubleInverse =
        new FuncValueConverter<double, double>(value => -value);
    
    public static readonly IValueConverter IntInverse =
        new FuncValueConverter<int, int>(value => -value);
    
    public static readonly IValueConverter FloatInverse =
        new FuncValueConverter<float, float>(value => -value);
    
    public static readonly IValueConverter LongInverse =
        new FuncValueConverter<long, long>(value => -value);
    
    public static readonly IValueConverter DecimalInverse =
        new FuncValueConverter<decimal, decimal>(value => -value);
}