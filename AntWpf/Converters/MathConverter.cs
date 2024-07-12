using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace AntWpf.Converters
{
    public enum MathOperation
    {
        Add,
        Divide,
        Multiply,
        Subtract,
        Pow
    }
    public sealed class MathConverter : MarkupExtension, IValueConverter
    {
        public MathOperation Operation { get; set; }

        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            try
            {
                double value1 = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                double value2 = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
                switch (Operation)
                {
                    case MathOperation.Add:
                        return value1 + value2;
                    case MathOperation.Divide:
                        return value1 / value2;
                    case MathOperation.Multiply:
                        return value1 * value2;
                    case MathOperation.Subtract:
                        return value1 - value2;
                    case MathOperation.Pow:
                        return Math.Pow(value1, value2);
                    default:
                        return Binding.DoNothing;
                }
            }
            catch (FormatException)
            {
                return Binding.DoNothing;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

       
    }

    public sealed class MathMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public MathOperation Operation { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double value1 = System.Convert.ToDouble(values[0], CultureInfo.InvariantCulture);
                double value2 = System.Convert.ToDouble(values[1], CultureInfo.InvariantCulture);
                switch (Operation)
                {
                    case MathOperation.Add:
                        return value1 + value2;
                    case MathOperation.Divide:
                        return value1 / value2;
                    case MathOperation.Multiply:
                        return value1 * value2;
                    case MathOperation.Subtract:
                        return value1 - value2;
                    case MathOperation.Pow:
                        return Math.Pow(value1, value2);
                    default:
                        return Binding.DoNothing;
                }
            }
            catch (FormatException)
            {
                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
