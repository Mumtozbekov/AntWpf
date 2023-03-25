namespace AntWpf.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(double), typeof(CornerRadius))]
    public class DoubleToCornerRadiusConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double))
            {
                return default(CornerRadius);
            }

            var d = (double)value;

            if (double.IsNaN(d) || d < 1)
            {
                d = 0;
            } else
            {
                // May require 50% rounded corners
                double.TryParse(parameter as string, out double val);

                if (val > 0)
                {
                    d /= val;
                }
            }
   
            return new CornerRadius(d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    [ValueConversion(typeof(CornerRadius), typeof(double))]
    public class CornerRadiusToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CornerRadius))
            {
                return default(double);
            }

            var cornerRadius = (CornerRadius)value;
            return Math.Max(cornerRadius.TopLeft, Math.Max(cornerRadius.TopRight, Math.Max(cornerRadius.BottomRight, cornerRadius.BottomLeft)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
