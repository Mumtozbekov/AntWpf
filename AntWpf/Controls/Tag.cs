namespace AntWpf.Controls
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Tag for categorizing or markup.
    /// </summary>
    [TemplatePart(Name = PART_Icon, Type = typeof(AntIcon))]
    [TemplatePart(Name = PART_Close, Type = typeof(UIElement))]
    public class Tag : ContentControl
    {
        #region Fields

        private const string PART_Icon = "PART_Icon";

        private const string PART_Close = "PART_Close";

        private AntIcon? icon;

        private UIElement? closeElement;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the tag is about to close. Set <see cref="CancelEventArgs.Cancel"/> to keep it open.
        /// </summary>
        public event EventHandler<CancelEventArgs>? Closing;

        /// <summary>
        /// Occurs after the tag has been closed (collapsed).
        /// </summary>
        public event EventHandler? Closed;

        #endregion

        #region Properties

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(TagColor), typeof(Tag), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the color of the tag: a preset (Success, Processing, Error, Warning, Default) or a custom brush.
        /// </summary>
        public TagColor? Color
        {
            get { return (TagColor?)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty =
            DependencyProperty.Register("Closable", typeof(bool), typeof(Tag), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets whether the tag can be closed.
        /// </summary>
        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty BorderedProperty =
            DependencyProperty.Register("Bordered", typeof(bool), typeof(Tag), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether the tag has a border.
        /// </summary>
        public bool Bordered
        {
            get { return (bool)GetValue(BorderedProperty); }
            set { SetValue(BorderedProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(AntIconKey?), typeof(Tag), new PropertyMetadata(null, OnIconChanged));

        /// <summary>
        /// Gets/sets the icon shown before the content.
        /// </summary>
        public AntIconKey? Icon
        {
            get { return (AntIconKey?)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Tag)d).UpdateIcon();
        }

        #endregion

        #region Constructors

        static Tag()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tag), new FrameworkPropertyMetadata(typeof(Tag)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            if (closeElement != null)
            {
                closeElement.MouseLeftButtonDown -= OnCloseMouseLeftButtonDown;
            }

            base.OnApplyTemplate();

            icon = GetTemplateChild(PART_Icon) as AntIcon;
            closeElement = GetTemplateChild(PART_Close) as UIElement;

            if (closeElement != null)
            {
                closeElement.MouseLeftButtonDown += OnCloseMouseLeftButtonDown;
            }

            UpdateIcon();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Closes the tag: raises <see cref="Closing"/>, collapses the tag unless cancelled and raises <see cref="Closed"/>.
        /// </summary>
        public void Close()
        {
            var args = new CancelEventArgs();
            Closing?.Invoke(this, args);

            if (args.Cancel) return;

            Visibility = Visibility.Collapsed;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private void OnCloseMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void UpdateIcon()
        {
            if (icon != null && Icon.HasValue)
            {
                icon.Key = Icon.Value;
            }
        }

        #endregion
    }
}
