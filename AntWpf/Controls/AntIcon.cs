using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Media;

namespace AntWpf.Controls
{
    public class AntIcon : System.Windows.Controls.Control, ISpinable
    {
        private static readonly Lazy<IDictionary<AntIconKey, string>> _dataList
        = new Lazy<IDictionary<AntIconKey, string>>(AntIconDataFactory.Create);

        /// <summary>
        /// Parsed and frozen geometries, shared by all <see cref="AntIcon"/> instances.
        /// </summary>
        private static readonly ConcurrentDictionary<AntIconKey, Geometry> _geometryCache
        = new ConcurrentDictionary<AntIconKey, Geometry>();

        static AntIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntIcon), new FrameworkPropertyMetadata(typeof(AntIcon)));

            // The icon is sized by FontSize (1em) when Width/Height are not set, so FontSize must re-measure.
            FontSizeProperty.OverrideMetadata(typeof(AntIcon), new FrameworkPropertyMetadata(
                SystemFonts.MessageFontSize,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        public AntIcon()
        {
            Loaded += (s, e) => UpdateSpin();
        }

        public static readonly DependencyProperty KeyProperty
        = DependencyProperty.Register(nameof(Key), typeof(AntIconKey), typeof(AntIcon), new PropertyMetadata(default(AntIconKey), KeyPropertyChangedCallback));

        private static void KeyPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var icon = (AntIcon)dependencyObject;
            icon.UpdateData();
            icon.UpdateSpin();
        }

        private static readonly DependencyPropertyKey DataPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Data), typeof(string), typeof(AntIcon), new PropertyMetadata(""));

        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey GeometryPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Geometry), typeof(Geometry), typeof(AntIcon), new PropertyMetadata(Geometry.Empty));

        public static readonly DependencyProperty GeometryProperty = GeometryPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SpinProperty
        = DependencyProperty.Register(nameof(Spin), typeof(bool?), typeof(AntIcon), new PropertyMetadata(null, OnSpinChanged));

        private static void OnSpinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((AntIcon)d).UpdateSpin();

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public AntIconKey Key
        {
            get => (AntIconKey)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }


        /// <summary>
        /// Gets the icon path data for the current <see cref="Key"/>.
        /// </summary>
        [TypeConverter(typeof(GeometryConverter))]
        public string? Data
        {
            get => (string?)GetValue(DataProperty);
            private set => SetValue(DataPropertyKey, value);
        }

        /// <summary>
        /// Gets the parsed, frozen geometry for the current <see cref="Key"/>.
        /// The same instance is shared by every icon with the same key.
        /// </summary>
        public Geometry Geometry
        {
            get => (Geometry)GetValue(GeometryProperty);
            private set => SetValue(GeometryPropertyKey, value);
        }

        /// <summary>
        /// Gets or sets whether the icon spins.
        /// <c>null</c> (default) spins only for <see cref="AntIconKey.LoadingOutlined"/>.
        /// </summary>
        public bool? Spin
        {
            get => (bool?)GetValue(SpinProperty);
            set => SetValue(SpinProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateData();
        }

        /// <summary>
        /// Without Width/Height the icon is FontSize x FontSize (1em).
        /// With only one of them set, the icon stays square.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            double width = Width, height = Height;

            if (double.IsNaN(width) && double.IsNaN(height))
            {
                width = height = FontSize;
            }
            else if (double.IsNaN(width))
            {
                width = height;
            }
            else if (double.IsNaN(height))
            {
                height = width;
            }

            var size = new Size(Math.Min(width, constraint.Width), Math.Min(height, constraint.Height));
            base.MeasureOverride(size);
            return size;
        }

        internal static Geometry GetGeometry(AntIconKey key)
            => _geometryCache.GetOrAdd(key, k =>
            {
                if (!_dataList.Value.TryGetValue(k, out var data) || string.IsNullOrEmpty(data))
                {
                    return Geometry.Empty;
                }

                var geometry = Geometry.Parse(data);

                if (geometry.CanFreeze)
                {
                    geometry.Freeze();
                }

                return geometry;
            });

        private void UpdateData()
        {
            string? data = null;
            _dataList.Value?.TryGetValue(Key, out data);
            Data = data;
            Geometry = GetGeometry(Key);
        }

        private void UpdateSpin()
        {
            if (Spin ?? Key == AntIconKey.LoadingOutlined)
            {
                this.BeginSpin(1d);
            }
            else
            {
                this.StopSpin();
            }
        }
    }
}
