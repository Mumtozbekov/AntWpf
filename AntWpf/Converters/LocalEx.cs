using System.Collections.Generic;
using System.Linq;

//https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/src/MaterialDesignThemes.Wpf/Converters/CircularProgressBar/LocalEx.cs

namespace AntWpf.Converters
{
    internal static class LocalEx
    {
        public static double ExtractDouble(this object value)
        {
            double d = value as double? ?? double.NaN;
            return double.IsInfinity(d) ? double.NaN : d;
        }


        public static bool AnyNan(this IEnumerable<double> values) => values.Any(double.IsNaN);
    }
}
