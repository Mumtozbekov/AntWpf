namespace AntWpf.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Attached properties and events for <see cref="TabControl"/> and <see cref="TabItem"/> styled with
    /// <c>Ant.Tabs</c> / <c>Ant.TabItem</c>.
    /// </summary>
    public static class Tabs
    {
        #region Fields

        private const string PART_InkBar = "PART_InkBar";

        private static readonly Duration InkBarDuration = TimeSpan.FromSeconds(0.3);

        private static readonly IEasingFunction InkBarEasing = new CubicEase { EasingMode = EasingMode.EaseInOut };

        #endregion

        #region Attached Properties

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.RegisterAttached("Type", typeof(TabsType), typeof(Tabs),
                new FrameworkPropertyMetadata(TabsType.Line, FrameworkPropertyMetadataOptions.Inherits, OnTypeChanged));

        /// <summary>
        /// Gets the type of the tabs: Line (default) or Card. Inherited by the tab items.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TabControl))]
        public static TabsType GetType(DependencyObject obj)
        {
            return (TabsType)obj.GetValue(TypeProperty);
        }

        /// <summary>
        /// Sets the type of the tabs.
        /// </summary>
        public static void SetType(DependencyObject obj, TabsType value)
        {
            obj.SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.RegisterAttached("Size", typeof(Sizes?), typeof(Tabs),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets the size of the tabs (null for default, Small or Large). Inherited by the tab items.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TabControl))]
        public static Sizes? GetSize(DependencyObject obj)
        {
            return (Sizes?)obj.GetValue(SizeProperty);
        }

        /// <summary>
        /// Sets the size of the tabs.
        /// </summary>
        public static void SetSize(DependencyObject obj, Sizes? value)
        {
            obj.SetValue(SizeProperty, value);
        }

        public static readonly DependencyProperty CenteredProperty =
            DependencyProperty.RegisterAttached("Centered", typeof(bool), typeof(Tabs), new PropertyMetadata(false));

        /// <summary>
        /// Gets whether the tab headers are centered (top and bottom placement).
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TabControl))]
        public static bool GetCentered(DependencyObject obj)
        {
            return (bool)obj.GetValue(CenteredProperty);
        }

        /// <summary>
        /// Sets whether the tab headers are centered.
        /// </summary>
        public static void SetCentered(DependencyObject obj, bool value)
        {
            obj.SetValue(CenteredProperty, value);
        }

        public static readonly DependencyProperty ClosableProperty =
            DependencyProperty.RegisterAttached("Closable", typeof(bool), typeof(Tabs),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets whether a card tab shows a close button. Set it on a TabItem, or on the TabControl for all items.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(TabItem))]
        [AttachedPropertyBrowsableForType(typeof(TabControl))]
        public static bool GetClosable(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClosableProperty);
        }

        /// <summary>
        /// Sets whether a card tab shows a close button.
        /// </summary>
        public static void SetClosable(DependencyObject obj, bool value)
        {
            obj.SetValue(ClosableProperty, value);
        }

        #endregion

        #region Routed Events and Commands

        /// <summary>
        /// Raised (bubbling, from the TabItem) when the close button of a closable card tab is clicked.
        /// The tab is not removed automatically: remove it from the TabControl in the handler.
        /// </summary>
        public static readonly RoutedEvent CloseRequestedEvent =
            EventManager.RegisterRoutedEvent("CloseRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Tabs));

        public static void AddCloseRequestedHandler(DependencyObject obj, RoutedEventHandler handler)
        {
            (obj as UIElement)?.AddHandler(CloseRequestedEvent, handler);
        }

        public static void RemoveCloseRequestedHandler(DependencyObject obj, RoutedEventHandler handler)
        {
            (obj as UIElement)?.RemoveHandler(CloseRequestedEvent, handler);
        }

        /// <summary>
        /// Command used by the close button of a tab item; raises <see cref="CloseRequestedEvent"/> on the TabItem.
        /// </summary>
        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand("CloseTab", typeof(Tabs));

        #endregion

        #region Constructors

        static Tabs()
        {
            CommandManager.RegisterClassCommandBinding(typeof(TabItem), new CommandBinding(CloseTabCommand, OnCloseTabExecuted));

            // Ink bar: follow the selected tab of TabControls whose template has PART_InkBar.
            EventManager.RegisterClassHandler(typeof(TabControl), Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnSelectionChanged));
            EventManager.RegisterClassHandler(typeof(TabControl), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnTabControlLayoutChanged));
            EventManager.RegisterClassHandler(typeof(TabControl), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTabControlLayoutChanged));
            EventManager.RegisterClassHandler(typeof(TabItem), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTabItemSizeChanged));
        }

        #endregion

        #region Private Methods

        private static void OnCloseTabExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is TabItem item)
            {
                item.RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, item));
                e.Handled = true;
            }
        }

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TabControl tabControl)
            {
                QueueInkBarUpdate(tabControl, animate: false);
            }
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignore selection changes bubbling up from selectors inside the tab content.
            if (sender is TabControl tabControl && ReferenceEquals(e.OriginalSource, tabControl))
            {
                QueueInkBarUpdate(tabControl, animate: true);
            }
        }

        private static void OnTabControlLayoutChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                QueueInkBarUpdate(tabControl, animate: false);
            }
        }

        private static void OnTabItemSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TabItem item && ItemsControl.ItemsControlFromItemContainer(item) is TabControl tabControl)
            {
                QueueInkBarUpdate(tabControl, animate: false);
            }
        }

        /// <summary>
        /// Updates after layout so the selected tab (and the header panel) have their final bounds.
        /// </summary>
        private static void QueueInkBarUpdate(TabControl tabControl, bool animate)
        {
            tabControl.Dispatcher.BeginInvoke(new Action(() => UpdateInkBar(tabControl, animate)), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void UpdateInkBar(TabControl tabControl, bool animate)
        {
            if (!(tabControl.Template?.FindName(PART_InkBar, tabControl) is FrameworkElement inkBar)
                || !(VisualTreeHelper.GetParent(inkBar) is Canvas host))
            {
                return;
            }

            var item = tabControl.SelectedItem as TabItem
                ?? tabControl.ItemContainerGenerator.ContainerFromItem(tabControl.SelectedItem) as TabItem;

            if (item == null || !item.IsVisible || !host.IsVisible || item.ActualWidth <= 0)
            {
                inkBar.Visibility = Visibility.Hidden;
                return;
            }

            Rect bounds;

            try
            {
                bounds = item.TransformToVisual(host).TransformBounds(new Rect(item.RenderSize));
            }
            catch (InvalidOperationException)
            {
                return;
            }

            var size = tabControl.TryFindResource("TabsInkBarSize") is double d ? d : 2.0;
            double left, top, width, height;

            switch (tabControl.TabStripPlacement)
            {
                case Dock.Bottom:
                    left = bounds.Left; top = bounds.Top; width = bounds.Width; height = size;
                    break;
                case Dock.Left:
                    left = bounds.Right - size; top = bounds.Top; width = size; height = bounds.Height;
                    break;
                case Dock.Right:
                    left = bounds.Left; top = bounds.Top; width = size; height = bounds.Height;
                    break;
                default:
                    left = bounds.Left; top = bounds.Bottom - size; width = bounds.Width; height = size;
                    break;
            }

            // Animate only when moving between tabs of an already placed bar.
            animate &= inkBar.Visibility == Visibility.Visible;
            inkBar.Visibility = Visibility.Visible;

            Move(inkBar, Canvas.LeftProperty, left, animate);
            Move(inkBar, Canvas.TopProperty, top, animate);
            Move(inkBar, FrameworkElement.WidthProperty, width, animate);
            Move(inkBar, FrameworkElement.HeightProperty, height, animate);
        }

        private static void Move(FrameworkElement element, DependencyProperty property, double to, bool animate)
        {
            if (animate)
            {
                element.BeginAnimation(property, new DoubleAnimation(to, InkBarDuration) { EasingFunction = InkBarEasing });
            }
            else
            {
                element.BeginAnimation(property, null);
                element.SetValue(property, to);
            }
        }

        #endregion
    }

    public enum TabsType : byte
    {
        Line, Card
    }
}
