﻿namespace AntWpf.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    public static class Input
    {
        #region Attached Properties

        public static readonly DependencyProperty PlaceholderProperty = 
            DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(Input), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets the placeholder for the input control.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        /// <summary>
        /// Sets the placeholder for the input control.
        /// </summary>
        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty PlaceholderBrushProperty =
            DependencyProperty.RegisterAttached("PlaceholderBrush", typeof(Brush), typeof(Input), 
                new FrameworkPropertyMetadata(
                    Brushes.Silver,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets the placeholder foreground brush for the input control.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static Brush GetPlaceholderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(PlaceholderBrushProperty);
        }

        /// <summary>
        /// Sets the placeholder foreground brush for the input control.
        /// </summary>
        public static void SetPlaceholderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(PlaceholderBrushProperty, value);
        }

        public static readonly DependencyProperty PrefixProperty = 
            DependencyProperty.RegisterAttached("Prefix", typeof(object), typeof(Input), new PropertyMetadata(null));

        /// <summary>
        /// Gets the prefix object of the input control.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static object GetPrefix(DependencyObject obj)
        {
            return obj.GetValue(PrefixProperty);
        }

        /// <summary>
        /// Sets the prefix object of the input control.
        /// </summary>
        public static void SetPrefix(DependencyObject obj, object value)
        {
            obj.SetValue(PrefixProperty, value);
        }

        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.RegisterAttached("Suffix", typeof(object), typeof(Input), new PropertyMetadata(null));

        /// <summary>
        /// Gets the suffix object of the input control.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static object GetSuffix(DependencyObject obj)
        {
            return obj.GetValue(SuffixProperty);
        }

        /// <summary>
        /// Sets the suffix object of the input control.
        /// </summary>
        public static void SetSuffix(DependencyObject obj, object value)
        {
            obj.SetValue(SuffixProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(Input),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnPasswordChanged)));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                var newVal = (string)e.NewValue;

                // Sync password
                if (newVal != passwordBox.Password)
                {
                    passwordBox.Password = newVal;
                }
            }
        }

        /// <summary>
        /// Gets the password currently held by the PasswordBox.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }

        /// <summary>
        /// Sets the password currently held by the PasswordBox.
        /// </summary>
        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty ClearableProperty =
            DependencyProperty.RegisterAttached("Clearable", typeof(bool), typeof(Input), new PropertyMetadata(false));

        /// <summary>
        /// Gets whether the input control has a clear behavior.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetClearable(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClearableProperty);
        }

        /// <summary>
        /// Sets whether the input control has a clear behavior.
        /// </summary>
        public static void SetClearable(DependencyObject obj, bool value)
        {
            obj.SetValue(ClearableProperty, value);
        }

        public static readonly DependencyProperty EyeableProperty = 
            DependencyProperty.RegisterAttached("Eyeable", typeof(bool), typeof(Input), new PropertyMetadata(false));

        /// <summary>
        /// Gets whether the password currently held by PasswordBox is displayed in text.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static bool GetEyeable(DependencyObject obj)
        {
            return (bool)obj.GetValue(EyeableProperty);
        }

        /// <summary>
        /// Sets whether the password currently held by PasswordBox is displayed in text.
        /// </summary>
        public static void SetEyeable(DependencyObject obj, bool value)
        {
            obj.SetValue(EyeableProperty, value);
        }

        public static readonly DependencyProperty ClearEnabledProperty = 
            DependencyProperty.RegisterAttached("ClearEnabled", typeof(bool), typeof(Input), new PropertyMetadata(false, OnClearEnabled));

        /// <summary>
        /// Gets the clear enable state of the control.
        /// </summary>
        public static bool GetClearEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClearEnabledProperty);
        }


        /// <summary>
        /// Sets the clear enable state of the control.
        /// </summary>
        public static void SetClearEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(ClearEnabledProperty, value);
        }

        private static void OnClearEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement)
            {
                if ((bool)e.NewValue)
                {
                    ((UIElement)d).MouseLeftButtonUp += OnClear;
                }
                else
                {
                    ((UIElement)d).MouseLeftButtonUp -= OnClear;
                }
            }
        }

        #endregion

        #region Private Methods

        private static void OnClear(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var parent = d.GetAncestors().FirstOrDefault(a => a is TextBox || a is PasswordBox || a is ComboBox);

                if (GetClearable(parent))
                {
                    if (parent is TextBox)
                    {
                        ((TextBox)parent).Clear();
                        ((TextBox)parent).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    }
                    else if (parent is PasswordBox)
                    {
                        ((PasswordBox)parent).Clear();
                        ((PasswordBox)parent).GetBindingExpression(PasswordProperty)?.UpdateSource();
                    }
                    else if (parent is ComboBox)
                    {
                        if (((ComboBox)parent).IsEditable)
                        {
                            ((ComboBox)parent).Text = string.Empty;
                            ((ComboBox)parent).GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
                        }

                        ((ComboBox)parent).SelectedItem = null;
                        ((ComboBox)parent).GetBindingExpression(Selector.SelectedItemProperty)?.UpdateSource();
                    }
                }
            }
        }

        #endregion
    }
}
