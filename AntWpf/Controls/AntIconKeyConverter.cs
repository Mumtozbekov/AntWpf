using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AntWpf.Controls
{
    /// <summary>
    /// Converts strings to <see cref="AntIconKey"/>.
    /// Accepts enum names (<c>SearchOutlined</c>) and the legacy <see cref="Icon"/> type names
    /// (<c>search</c> → SearchOutlined, <c>check-circle</c> → CheckCircleOutlined,
    /// <c>check-circle.fill</c> → CheckCircleFilled), so existing XAML such as
    /// <c>Icon="search"</c> keeps working.
    /// </summary>
    public class AntIconKeyConverter : EnumConverter
    {
        private const string FillSuffix = ".fill";

        public AntIconKeyConverter() : base(typeof(AntIconKey))
        {
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string text && TryParse(text, out var key))
            {
                return key;
            }

            return base.ConvertFrom(context, culture, value);
        }

        internal static bool TryParse(string? text, out AntIconKey key)
        {
            key = default;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            if (IsDefinedName(text, out key))
            {
                return true;
            }

            // Legacy kebab-case name: "check-circle" → "CheckCircleOutlined", "check-circle.fill" → "CheckCircleFilled".
            var suffix = "Outlined";

            if (text.EndsWith(FillSuffix, StringComparison.OrdinalIgnoreCase))
            {
                suffix = "Filled";
                text = text.Substring(0, text.Length - FillSuffix.Length);
            }

            var name = new StringBuilder();

            foreach (var part in text.Split('-', StringSplitOptions.RemoveEmptyEntries))
            {
                name.Append(char.ToUpperInvariant(part[0])).Append(part, 1, part.Length - 1);
            }

            return name.Length > 0 && IsDefinedName(name.Append(suffix).ToString(), out key);
        }

        private static bool IsDefinedName(string name, out AntIconKey key)
            => Enum.TryParse(name, true, out key) && Enum.IsDefined(typeof(AntIconKey), key);
    }
}
