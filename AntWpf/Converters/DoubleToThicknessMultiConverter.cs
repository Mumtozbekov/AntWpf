using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace AntWpf.Converters
{
    public class DoubleToThicknessMultiConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var parent = values[0] as double?;
            var self = values[1] as double?;

            if (parent == null || self == null)
            {
                return default(Thickness);
            }

            double.TryParse(parameter as string, out double param);
            return new Thickness(parent.Value - self.Value - param, 0.0, 0.0, 0.0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => DependencyProperty.UnsetValue).ToArray();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
