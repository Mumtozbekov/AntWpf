using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

//https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/src/MaterialDesignThemes.Wpf/Converters/CircularProgressBar/ArcSizeConverter.cs


namespace AntWpf.Converters
{
    public class ArcSizeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is double v && (v > 0.0) ? new Size(v / 2, v / 2) : new Point();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
