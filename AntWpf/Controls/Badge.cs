namespace AntWpf.Controls
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Small numerical value or status descriptor shown on the top right corner of its content.
    /// </summary>
    [TemplatePart(Name = PART_Indicator, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_Count, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_Status, Type = typeof(FrameworkElement))]
    public class Badge : ContentControl
    {
        #region Fields

        private const string PART_Indicator = "PART_Indicator";

        private const string PART_Count = "PART_Count";

        private const string PART_Status = "PART_Status";

        private static readonly Duration ZoomDuration = TimeSpan.FromSeconds(0.3);

        private static readonly Duration PulseDuration = TimeSpan.FromSeconds(0.2);

        private readonly ScaleTransform scale = new ScaleTransform(1, 1);

        private readonly TranslateTransform translate = new TranslateTransform();

        private FrameworkElement? indicator;

        private TextBlock? countText;

        private FrameworkElement? status;

        private bool isIndicatorShown;

        #endregion

        #region Properties

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(Badge), new PropertyMetadata(0, OnCountChanged));

        /// <summary>
        /// Gets/sets the number to show in the badge.
        /// </summary>
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty OverflowCountProperty =
            DependencyProperty.Register("OverflowCount", typeof(int), typeof(Badge), new PropertyMetadata(99, OnBadgeChanged));

        /// <summary>
        /// Gets/sets the max count to show; larger counts are shown as "OverflowCount+".
        /// </summary>
        public int OverflowCount
        {
            get { return (int)GetValue(OverflowCountProperty); }
            set { SetValue(OverflowCountProperty, value); }
        }

        public static readonly DependencyProperty ShowZeroProperty =
            DependencyProperty.Register("ShowZero", typeof(bool), typeof(Badge), new PropertyMetadata(false, OnBadgeChanged));

        /// <summary>
        /// Gets/sets whether to show the badge when <see cref="Count"/> is zero.
        /// </summary>
        public bool ShowZero
        {
            get { return (bool)GetValue(ShowZeroProperty); }
            set { SetValue(ShowZeroProperty, value); }
        }

        public static readonly DependencyProperty DotProperty =
            DependencyProperty.Register("Dot", typeof(bool), typeof(Badge), new PropertyMetadata(false, OnBadgeChanged));

        /// <summary>
        /// Gets/sets whether to show a red dot instead of the count.
        /// </summary>
        public bool Dot
        {
            get { return (bool)GetValue(DotProperty); }
            set { SetValue(DotProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(BadgeStatus?), typeof(Badge), new PropertyMetadata(null, OnBadgeChanged));

        /// <summary>
        /// Gets/sets the status. Without content the badge shows a status dot followed by <see cref="Text"/>.
        /// </summary>
        public BadgeStatus? Status
        {
            get { return (BadgeStatus?)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Badge), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the text shown next to the status dot.
        /// </summary>
        public string? Text
        {
            get { return (string?)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Point), typeof(Badge), new PropertyMetadata(default(Point), OnOffsetChanged));

        /// <summary>
        /// Gets/sets the offset of the badge from the top right corner (x to the right, y down).
        /// </summary>
        public Point Offset
        {
            get { return (Point)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Sizes?), typeof(Badge), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the size of the badge (null for default, or Small).
        /// </summary>
        public Sizes? Size
        {
            get { return (Sizes?)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).UpdateBadge(animate: true, countChanged: true);
        }

        private static void OnBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).UpdateBadge(animate: true, countChanged: false);
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).UpdatePosition();
        }

        #endregion

        #region Constructors

        static Badge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            if (indicator != null)
            {
                indicator.SizeChanged -= OnIndicatorSizeChanged;
            }

            base.OnApplyTemplate();

            indicator = GetTemplateChild(PART_Indicator) as FrameworkElement;
            countText = GetTemplateChild(PART_Count) as TextBlock;
            status = GetTemplateChild(PART_Status) as FrameworkElement;

            if (indicator != null)
            {
                var transform = new TransformGroup();
                transform.Children.Add(scale);
                transform.Children.Add(translate);

                indicator.RenderTransform = transform;
                indicator.RenderTransformOrigin = new Point(0.5, 0.5);
                indicator.SizeChanged += OnIndicatorSizeChanged;
            }

            isIndicatorShown = false;
            UpdateBadge(animate: false, countChanged: false);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            UpdateBadge(animate: false, countChanged: false);
            UpdatePosition();
        }

        #endregion

        #region Private Methods

        private bool IsStandaloneStatus => Content == null && Status.HasValue;

        private bool HasIndicator => !IsStandaloneStatus && (Dot || Count != 0 || ShowZero);

        private string DisplayCount => Count > OverflowCount
            ? OverflowCount.ToString(CultureInfo.CurrentCulture) + "+"
            : Count.ToString(CultureInfo.CurrentCulture);

        private void UpdateBadge(bool animate, bool countChanged)
        {
            if (status != null)
            {
                status.Visibility = IsStandaloneStatus ? Visibility.Visible : Visibility.Collapsed;
            }

            if (countText != null)
            {
                countText.Text = DisplayCount;
            }

            if (indicator == null) return;

            animate &= IsLoaded;
            bool show = HasIndicator;

            if (show == isIndicatorShown)
            {
                if (show && animate && countChanged && !Dot)
                {
                    // Small bump when the number changes.
                    Animate(1.2, 1, PulseDuration, new CubicEase { EasingMode = EasingMode.EaseOut }, null);
                }

                return;
            }

            isIndicatorShown = show;

            if (show)
            {
                indicator.Visibility = Visibility.Visible;

                if (animate)
                {
                    Animate(0, 1, ZoomDuration, new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }, null);
                }
                else
                {
                    Animate(1, 1, new Duration(TimeSpan.Zero), null, null);
                }
            }
            else if (animate)
            {
                Animate(1, 0, ZoomDuration, new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.5 }, () =>
                {
                    if (!isIndicatorShown && indicator != null)
                    {
                        indicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            else
            {
                indicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Animate(double from, double to, Duration duration, IEasingFunction? easing, Action? completed)
        {
            var animation = new DoubleAnimation(from, to, duration) { EasingFunction = easing };

            if (completed != null)
            {
                animation.Completed += (s, e) => completed();
            }

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void OnIndicatorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePosition();
        }

        /// <summary>
        /// Centers the indicator on the top right corner of the content (translate 50%, -50%), plus <see cref="Offset"/>.
        /// </summary>
        private void UpdatePosition()
        {
            if (indicator == null) return;

            if (Content == null)
            {
                translate.X = 0;
                translate.Y = 0;
                return;
            }

            translate.X = indicator.ActualWidth / 2 + Offset.X;
            translate.Y = -indicator.ActualHeight / 2 + Offset.Y;
        }

        #endregion
    }

    public enum BadgeStatus : byte
    {
        Success, Processing, Default, Error, Warning
    }
}
