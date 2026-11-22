namespace AntWpf.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Alert component for feedback.
    /// </summary>
    [TemplatePart(Name = PART_Icon, Type = typeof(AntIcon))]
    [TemplatePart(Name = PART_Close, Type = typeof(UIElement))]
    public class Alert : ContentControl
    {
        #region Fields

        private const string PART_Icon = "PART_Icon";

        private const string PART_Close = "PART_Close";

        private static readonly Duration CloseDuration = TimeSpan.FromSeconds(0.3);

        private AntIcon? icon;

        private UIElement? closeElement;

        private bool isClosing;

        #endregion

        #region Events

        /// <summary>
        /// Occurs after the close animation has finished and the alert is collapsed.
        /// </summary>
        public event EventHandler? Closed;

        #endregion

        #region Properties

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(AlertType?), typeof(Alert), new PropertyMetadata(null, OnIconChanged));

        /// <summary>
        /// Gets/sets the type of the alert. Null means Info (Warning for a banner).
        /// </summary>
        public AlertType? Type
        {
            get { return (AlertType?)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(object), typeof(Alert), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the message. When null, <see cref="ContentControl.Content"/> is shown as the message.
        /// </summary>
        public object Message
        {
            get { return GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(object), typeof(Alert), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the additional description. With a description the icon is larger and the message is a heading.
        /// </summary>
        public object Description
        {
            get { return GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register("ShowIcon", typeof(bool?), typeof(Alert), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets whether to show the type icon. Null shows it only for a banner.
        /// </summary>
        public bool? ShowIcon
        {
            get { return (bool?)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty =
            DependencyProperty.Register("Closable", typeof(bool), typeof(Alert), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets whether the alert can be closed.
        /// </summary>
        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty BannerProperty =
            DependencyProperty.Register("Banner", typeof(bool), typeof(Alert), new PropertyMetadata(false, OnIconChanged));

        /// <summary>
        /// Gets/sets whether to show the alert as a banner (no border and radius, warning by default).
        /// </summary>
        public bool Banner
        {
            get { return (bool)GetValue(BannerProperty); }
            set { SetValue(BannerProperty, value); }
        }

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register("Action", typeof(object), typeof(Alert), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the content shown on the right side, e.g. a button.
        /// </summary>
        public object Action
        {
            get { return GetValue(ActionProperty); }
            set { SetValue(ActionProperty, value); }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Alert)d).UpdateIcon();
        }

        #endregion

        #region Constructors

        static Alert()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Alert), new FrameworkPropertyMetadata(typeof(Alert)));
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
        /// Closes the alert: animates height and opacity to zero, collapses it and raises <see cref="Closed"/>.
        /// </summary>
        public void Close()
        {
            if (isClosing || Visibility != Visibility.Visible) return;

            isClosing = true;

            var ease = new CircleEase { EasingMode = EasingMode.EaseInOut };
            var height = new DoubleAnimation(ActualHeight, 0, CloseDuration) { EasingFunction = ease };
            height.Completed += OnCloseCompleted;

            BeginAnimation(HeightProperty, height);
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, CloseDuration) { EasingFunction = ease });
            BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0), CloseDuration) { EasingFunction = ease });
        }

        #endregion

        #region Private Methods

        private void OnCloseCompleted(object? sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;

            // Drop the animations so the alert has its normal size if it is shown again.
            BeginAnimation(HeightProperty, null);
            BeginAnimation(OpacityProperty, null);
            BeginAnimation(MarginProperty, null);

            isClosing = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void UpdateIcon()
        {
            if (icon == null) return;

            var type = Type ?? (Banner ? AlertType.Warning : AlertType.Info);

            switch (type)
            {
                case AlertType.Success: icon.Key = AntIconKey.CheckCircleFilled; break;
                case AlertType.Warning: icon.Key = AntIconKey.ExclamationCircleFilled; break;
                case AlertType.Error: icon.Key = AntIconKey.CloseCircleFilled; break;
                default: icon.Key = AntIconKey.InfoCircleFilled; break;
            }
        }

        #endregion
    }

    public enum AlertType : byte
    {
        Success, Info, Warning, Error
    }
}
