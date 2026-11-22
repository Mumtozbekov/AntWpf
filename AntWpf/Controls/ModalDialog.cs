namespace AntWpf.Controls
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    /// <summary>
    /// The visual of a <see cref="Modal"/> (and of <see cref="Modal.Confirm"/> and friends): mask and dialog,
    /// shown in the window overlay. Styled in Themes/Modal.xaml.
    /// </summary>
    [TemplatePart(Name = PART_Mask, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_Wrapper, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_Panel, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = PART_Ok, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_Cancel, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_Close, Type = typeof(ButtonBase))]
    public class ModalDialog : System.Windows.Controls.Control
    {
        #region Fields

        private const string PART_Mask = "PART_Mask";

        private const string PART_Wrapper = "PART_Wrapper";

        private const string PART_Panel = "PART_Panel";

        private const string PART_Ok = "PART_Ok";

        private const string PART_Cancel = "PART_Cancel";

        private const string PART_Close = "PART_Close";

        private static readonly Duration MaskDuration = TimeSpan.FromSeconds(0.2);

        private static readonly Duration OpenDuration = TimeSpan.FromSeconds(0.3);

        private static readonly Duration CloseDuration = TimeSpan.FromSeconds(0.2);

        private UIElement? mask;

        private UIElement? wrapper;

        private FrameworkElement? panel;

        private ButtonBase? okButton;

        private ButtonBase? cancelButton;

        private ButtonBase? closeButton;

        private OverlayLayer? layer;

        private IInputElement? previousFocus;

        private bool isClosing;

        #endregion

        #region Events

        internal event EventHandler? OkRequested;

        internal event EventHandler? CancelRequested;

        internal event EventHandler? Closed;

        #endregion

        #region Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(ModalDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the title.
        /// </summary>
        public object? Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(object), typeof(ModalDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the body content.
        /// </summary>
        public object? Body
        {
            get { return GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty BodyTemplateProperty =
            DependencyProperty.Register("BodyTemplate", typeof(DataTemplate), typeof(ModalDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the template of the body content.
        /// </summary>
        public DataTemplate BodyTemplate
        {
            get { return (DataTemplate)GetValue(BodyTemplateProperty); }
            set { SetValue(BodyTemplateProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(object), typeof(ModalDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets a custom footer. When null the OK and Cancel buttons are shown.
        /// </summary>
        public object? Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty OkTextProperty =
            DependencyProperty.Register("OkText", typeof(string), typeof(ModalDialog), new PropertyMetadata("OK"));

        /// <summary>
        /// Gets/sets the text of the OK button.
        /// </summary>
        public string OkText
        {
            get { return (string)GetValue(OkTextProperty); }
            set { SetValue(OkTextProperty, value); }
        }

        public static readonly DependencyProperty CancelTextProperty =
            DependencyProperty.Register("CancelText", typeof(string), typeof(ModalDialog), new PropertyMetadata("Cancel"));

        /// <summary>
        /// Gets/sets the text of the Cancel button.
        /// </summary>
        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        public static readonly DependencyProperty OkButtonLoadingProperty =
            DependencyProperty.Register("OkButtonLoading", typeof(bool), typeof(ModalDialog), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets the loading state of the OK button.
        /// </summary>
        public bool OkButtonLoading
        {
            get { return (bool)GetValue(OkButtonLoadingProperty); }
            set { SetValue(OkButtonLoadingProperty, value); }
        }

        public static readonly DependencyProperty ShowCancelProperty =
            DependencyProperty.Register("ShowCancel", typeof(bool), typeof(ModalDialog), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether the default footer has a Cancel button.
        /// </summary>
        public bool ShowCancel
        {
            get { return (bool)GetValue(ShowCancelProperty); }
            set { SetValue(ShowCancelProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty =
            DependencyProperty.Register("Closable", typeof(bool), typeof(ModalDialog), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether the close (x) button is shown.
        /// </summary>
        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty MaskClosableProperty =
            DependencyProperty.Register("MaskClosable", typeof(bool), typeof(ModalDialog), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether clicking the mask cancels the dialog.
        /// </summary>
        public bool MaskClosable
        {
            get { return (bool)GetValue(MaskClosableProperty); }
            set { SetValue(MaskClosableProperty, value); }
        }

        public static readonly DependencyProperty KeyboardProperty =
            DependencyProperty.Register("Keyboard", typeof(bool), typeof(ModalDialog), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether Esc cancels the dialog.
        /// </summary>
        public bool Keyboard
        {
            get { return (bool)GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        public static readonly DependencyProperty CenteredProperty =
            DependencyProperty.Register("Centered", typeof(bool), typeof(ModalDialog), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets whether the dialog is vertically centered (otherwise 100px from the top).
        /// </summary>
        public bool Centered
        {
            get { return (bool)GetValue(CenteredProperty); }
            set { SetValue(CenteredProperty, value); }
        }

        public static readonly DependencyProperty DialogWidthProperty =
            DependencyProperty.Register("DialogWidth", typeof(double), typeof(ModalDialog), new PropertyMetadata(520.0));

        /// <summary>
        /// Gets/sets the width of the dialog.
        /// </summary>
        public double DialogWidth
        {
            get { return (double)GetValue(DialogWidthProperty); }
            set { SetValue(DialogWidthProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(ModalType?), typeof(ModalDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the confirm layout (icon, title and content) used by <see cref="Modal.Confirm"/> and friends; null for a normal dialog.
        /// </summary>
        public ModalType? Type
        {
            get { return (ModalType?)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        #endregion

        #region Constructors

        static ModalDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModalDialog), new FrameworkPropertyMetadata(typeof(ModalDialog)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            if (mask != null) mask.MouseLeftButtonDown -= OnMaskMouseLeftButtonDown;
            wrapper?.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMaskMouseLeftButtonDown));
            if (okButton != null) okButton.Click -= OnOkClick;
            if (cancelButton != null) cancelButton.Click -= OnCancelClick;
            if (closeButton != null) closeButton.Click -= OnCancelClick;

            base.OnApplyTemplate();

            mask = GetTemplateChild(PART_Mask) as UIElement;
            wrapper = GetTemplateChild(PART_Wrapper) as UIElement;
            panel = GetTemplateChild(PART_Panel) as FrameworkElement;

            // The ScrollViewer around the panel takes every click over the mask, so mask clicks are detected on it.
            wrapper?.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMaskMouseLeftButtonDown), handledEventsToo: true);
            okButton = GetTemplateChild(PART_Ok) as ButtonBase;
            cancelButton = GetTemplateChild(PART_Cancel) as ButtonBase;
            closeButton = GetTemplateChild(PART_Close) as ButtonBase;

            if (mask != null) mask.MouseLeftButtonDown += OnMaskMouseLeftButtonDown;
            if (okButton != null) okButton.Click += OnOkClick;
            if (cancelButton != null) cancelButton.Click += OnCancelClick;
            if (closeButton != null) closeButton.Click += OnCancelClick;

            if (panel != null)
            {
                panel.RenderTransform = new ScaleTransform(1, 1);
                panel.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && Keyboard && !e.Handled)
            {
                e.Handled = true;
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds the dialog to the overlay of <paramref name="window"/> and plays the open animation.
        /// </summary>
        internal bool Show(Window window)
        {
            layer = OverlayLayer.Get(window);

            if (layer == null) return false;

            previousFocus = System.Windows.Input.Keyboard.FocusedElement;

            // Stack above other open dialogs, below messages.
            Panel.SetZIndex(this, 100 + layer.Root.Children.OfType<ModalDialog>().Count());
            layer.Root.Children.Add(this);

            Dispatcher.BeginInvoke(new Action(AnimateIn), DispatcherPriority.Loaded);
            return true;
        }

        /// <summary>
        /// Plays the close animation, removes the dialog, restores the focus and raises <see cref="Closed"/>.
        /// </summary>
        internal void Close()
        {
            if (isClosing || layer == null) return;

            isClosing = true;

            var ease = new CircleEase { EasingMode = EasingMode.EaseInOut };
            mask?.BeginAnimation(OpacityProperty, new DoubleAnimation(0, CloseDuration) { EasingFunction = ease });

            if (panel != null)
            {
                var done = new DoubleAnimation(0, CloseDuration) { EasingFunction = ease };
                done.Completed += (s, e) => Remove();
                panel.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.2, CloseDuration) { EasingFunction = ease });
                panel.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.2, CloseDuration) { EasingFunction = ease });
                panel.BeginAnimation(OpacityProperty, done);
            }
            else
            {
                Remove();
            }
        }

        /// <summary>
        /// Removes the dialog immediately (no animation), e.g. when its Modal is unloaded.
        /// </summary>
        internal void RemoveNow()
        {
            isClosing = true;
            Remove();
        }

        #endregion

        #region Private Methods

        private void AnimateIn()
        {
            if (isClosing) return;

            var ease = new CircleEase { EasingMode = EasingMode.EaseOut };
            mask?.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, MaskDuration));

            if (panel != null)
            {
                panel.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.2, 1, OpenDuration) { EasingFunction = ease });
                panel.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.2, 1, OpenDuration) { EasingFunction = ease });
                panel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, OpenDuration) { EasingFunction = ease });
            }

            // Move the focus into the dialog: the OK button when present, otherwise the dialog panel.
            if (okButton != null && okButton.IsVisible && okButton.Focus()) return;

            panel?.Focus();
        }

        private void Remove()
        {
            if (layer == null) return;

            layer.Root.Children.Remove(this);
            layer.RemoveIfEmpty();
            layer = null;

            if (previousFocus is DependencyObject element && PresentationSource.FromDependencyObject(element) != null)
            {
                previousFocus.Focus();
            }

            previousFocus = null;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Mouse events can come from content elements (e.g. a Run in a TextBlock): use the nearest visual.
        /// </summary>
        private static Visual? ToVisual(DependencyObject? element)
        {
            while (element != null && !(element is Visual))
            {
                element = LogicalTreeHelper.GetParent(element);
            }

            return element as Visual;
        }

        private static bool HasScrollBarAncestor(Visual element)
        {
            for (DependencyObject? current = element; current != null; current = VisualTreeHelper.GetParent(current))
            {
                if (current is ScrollBar) return true;
            }

            return false;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            OkRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnMaskMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clicks inside the dialog panel (or on the scroll bar) are not mask clicks.
            var source = ToVisual(e.OriginalSource as DependencyObject);

            if (source != null
                && ((panel != null && (ReferenceEquals(source, panel) || panel.IsAncestorOf(source))) || HasScrollBarAncestor(source)))
            {
                return;
            }

            e.Handled = true;

            if (MaskClosable)
            {
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }

    public enum ModalType : byte
    {
        Confirm, Info, Success, Error, Warning
    }
}
