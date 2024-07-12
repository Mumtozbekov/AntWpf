namespace AntWpf.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(Thickness), typeof(double))]
    public class ThicknessToDoubleConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness))
            {
                return default(double);
            }

            var thickness = (Thickness)value;
            var d = Math.Max(thickness.Left, Math.Max(thickness.Top, Math.Max(thickness.Right, thickness.Bottom)));

            // If it is a shape, it may be necessary to discard approximately 0.2px to remove the sawtooth.
            // Recorded in: Window10 x64, xiaomi laptop pro15.6
            // The cause of this problem is DPI.
            //if (bool.Parse(parameter as string))
            //{
            //    d -= d * 0.176;
            //}

            return d;
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

    
}
