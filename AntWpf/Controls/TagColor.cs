namespace AntWpf.Controls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Media;

    /// <summary>
    /// Color of a <see cref="Tag"/>: one of the <see cref="TagPreset"/> presets or a custom brush.
    /// In XAML it can be written as a preset name (<c>Success</c>) or as a brush (<c>#F50</c>, <c>Red</c>).
    /// </summary>
    [TypeConverter(typeof(TagColorConverter))]
    public sealed class TagColor
    {
        #region Fields

        public static readonly TagColor Default = new TagColor(TagPreset.Default, null);

        public static readonly TagColor Success = new TagColor(TagPreset.Success, null);

        public static readonly TagColor Processing = new TagColor(TagPreset.Processing, null);

        public static readonly TagColor Error = new TagColor(TagPreset.Error, null);

        public static readonly TagColor Warning = new TagColor(TagPreset.Warning, null);

        #endregion

        #region Constructors

        private TagColor(TagPreset? preset, Brush? brush)
        {
            Preset = preset;
            Brush = brush;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the preset, or null for a custom brush.
        /// </summary>
        public TagPreset? Preset { get; }

        /// <summary>
        /// Gets the custom brush, or null for a preset.
        /// </summary>
        public Brush? Brush { get; }

        /// <summary>
        /// Gets whether this color is a custom brush.
        /// </summary>
        public bool IsCustom => Brush != null;

        #endregion

        #region Public Methods

        public static TagColor FromPreset(TagPreset preset)
        {
            switch (preset)
            {
                case TagPreset.Success: return Success;
                case TagPreset.Processing: return Processing;
                case TagPreset.Error: return Error;
                case TagPreset.Warning: return Warning;
                default: return Default;
            }
        }

        public static TagColor FromBrush(Brush brush)
        {
            if (brush == null)
            {
                throw new ArgumentNullException(nameof(brush));
            }

            return new TagColor(null, brush);
        }

        /// <summary>
        /// Parses a preset name (case-insensitive) or a brush string such as <c>#F50</c> or <c>Red</c>.
        /// </summary>
        public static TagColor Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("Tag color must not be empty.");
            }

            value = value.Trim();

            foreach (TagPreset preset in Enum.GetValues(typeof(TagPreset)))
            {
                if (string.Equals(preset.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    return FromPreset(preset);
                }
            }

            var brush = (Brush?)new BrushConverter().ConvertFromInvariantString(value)
                ?? throw new FormatException($"'{value}' is not a tag preset or a brush.");

            return FromBrush(brush);
        }

        public static implicit operator TagColor(TagPreset preset) => FromPreset(preset);

        public static implicit operator TagColor(Brush brush) => FromBrush(brush);

        public override string ToString()
        {
            return Preset?.ToString() ?? Brush?.ToString() ?? string.Empty;
        }

        #endregion
    }

    public enum TagPreset : byte
    {
        Default, Success, Processing, Error, Warning
    }

    public sealed class TagColorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string)
                || typeof(Brush).IsAssignableFrom(sourceType)
                || sourceType == typeof(TagPreset)
                || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            switch (value)
            {
                case string text: return TagColor.Parse(text);
                case Brush brush: return TagColor.FromBrush(brush);
                case TagPreset preset: return TagColor.FromPreset(preset);
                default: return base.ConvertFrom(context, culture, value);
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is TagColor color)
            {
                return color.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
