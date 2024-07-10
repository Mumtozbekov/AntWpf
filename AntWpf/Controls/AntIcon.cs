using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Media;

namespace AntWpf.Controls
{
    public class AntIcon : System.Windows.Controls.Control
    {
        private static readonly Lazy<IDictionary<AntIconKey, string>> _dataList
        = new Lazy<IDictionary<AntIconKey, string>>(AntIconDataFactory.Create);

        static AntIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntIcon), new FrameworkPropertyMetadata(typeof(AntIcon)));
        }

        public static readonly DependencyProperty KeyProperty
        = DependencyProperty.Register(nameof(Key), typeof(AntIconKey), typeof(AntIcon), new PropertyMetadata(default(AntIconKey), KeyPropertyChangedCallback));

        private static void KeyPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
            => ((AntIcon)dependencyObject).UpdateData();

        private static readonly DependencyPropertyKey DataPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Data), typeof(string), typeof(AntIcon), new PropertyMetadata(""));

        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

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
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateData();
        }

        private void UpdateData()
        {
            string? data = null;
            _dataList.Value?.TryGetValue(Key, out data);
            Data = data;
        }
    }
}
