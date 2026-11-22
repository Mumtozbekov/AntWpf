namespace AntWpf.Controls
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;

    /// <summary>
    /// Global feedback messages shown at the top center of the active window.
    /// All methods can be called from any thread.
    /// </summary>
    public static class Message
    {
        #region Fields

        private static readonly object sync = new object();

        private static readonly Duration AnimationDuration = TimeSpan.FromSeconds(0.3);

        private static readonly ConditionalWeakTable<OverlayLayer, StackPanel> hosts = new ConditionalWeakTable<OverlayLayer, StackPanel>();

        private static double top = 8;

        private static TimeSpan defaultDuration = TimeSpan.FromSeconds(3);

        private static int? maxCount;

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows an info message. Dispose the result to close it early.
        /// </summary>
        /// <param name="content">Text or content of the message. UI elements must be created on the UI thread.</param>
        /// <param name="duration">Time before it closes; <see cref="TimeSpan.Zero"/> keeps it open. Defaults to 3 seconds (see <see cref="Config"/>).</param>
        public static IDisposable Info(object content, TimeSpan? duration = null) => Show(MessageType.Info, content, duration);

        /// <summary>
        /// Shows a success message. Dispose the result to close it early.
        /// </summary>
        public static IDisposable Success(object content, TimeSpan? duration = null) => Show(MessageType.Success, content, duration);

        /// <summary>
        /// Shows a warning message. Dispose the result to close it early.
        /// </summary>
        public static IDisposable Warning(object content, TimeSpan? duration = null) => Show(MessageType.Warning, content, duration);

        /// <summary>
        /// Shows an error message. Dispose the result to close it early.
        /// </summary>
        public static IDisposable Error(object content, TimeSpan? duration = null) => Show(MessageType.Error, content, duration);

        /// <summary>
        /// Shows a loading message with a spinning icon. Dispose the result to close it.
        /// </summary>
        public static IDisposable Loading(object content, TimeSpan? duration = null) => Show(MessageType.Loading, content, duration);

        /// <summary>
        /// Configures messages shown after the call. Null keeps the current value.
        /// </summary>
        /// <param name="top">Distance from the top of the window (default 8).</param>
        /// <param name="duration">Default duration (default 3 seconds; <see cref="TimeSpan.Zero"/> keeps messages open).</param>
        /// <param name="maxCount">Max number of messages at once, the oldest is closed first; 0 or less means no limit.</param>
        public static void Config(double? top = null, TimeSpan? duration = null, int? maxCount = null)
        {
            lock (sync)
            {
                if (top.HasValue) Message.top = top.Value;
                if (duration.HasValue) defaultDuration = duration.Value;
                if (maxCount.HasValue) Message.maxCount = maxCount.Value > 0 ? maxCount : null;
            }
        }

        #endregion

        #region Private Methods

        private static IDisposable Show(MessageType type, object content, TimeSpan? duration)
        {
            var dispatcher = Application.Current?.Dispatcher
                ?? throw new InvalidOperationException("Message requires an Application instance.");

            TimeSpan effectiveDuration;
            double effectiveTop;
            int? effectiveMaxCount;

            lock (sync)
            {
                effectiveDuration = duration ?? defaultDuration;
                effectiveTop = top;
                effectiveMaxCount = maxCount;
            }

            var notice = new Notice(dispatcher);
            dispatcher.BeginInvoke(new Action(() => notice.Open(type, content, effectiveDuration, effectiveTop, effectiveMaxCount)));
            return notice;
        }

        private static StackPanel GetHost(OverlayLayer layer)
        {
            if (hosts.TryGetValue(layer, out var host) && host.Parent == layer.Root)
            {
                return host;
            }

            hosts.Remove(layer);
            host = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            // Above the Modal mask and dialogs.
            Panel.SetZIndex(host, 1000);
            layer.Root.Children.Add(host);
            hosts.Add(layer, host);
            return host;
        }

        #endregion

        #region Notice

        /// <summary>
        /// Handle of one message. Created on the caller's thread, everything else runs on the dispatcher.
        /// </summary>
        private sealed class Notice : IDisposable
        {
            private readonly Dispatcher dispatcher;

            private int disposed;

            private OverlayLayer? layer;

            private StackPanel? host;

            private MessageItem? item;

            private DispatcherTimer? timer;

            private bool closing;

            public Notice(Dispatcher dispatcher)
            {
                this.dispatcher = dispatcher;
            }

            public bool IsClosing => closing;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 1) return;

                if (dispatcher.CheckAccess())
                {
                    Close();
                }
                else
                {
                    dispatcher.BeginInvoke(new Action(Close));
                }
            }

            public void Open(MessageType type, object content, TimeSpan duration, double top, int? maxCount)
            {
                // Disposed before it was shown.
                if (Volatile.Read(ref disposed) == 1) return;

                var window = OverlayLayer.GetActiveWindow();

                if (window == null)
                {
                    Trace.TraceWarning("AntWpf: no window to show the message in.");
                    return;
                }

                layer = OverlayLayer.Get(window);

                if (layer == null) return;

                host = GetHost(layer);
                host.Margin = new Thickness(0, top, 0, 0);

                if (maxCount.HasValue)
                {
                    var open = host.Children.OfType<MessageItem>()
                        .Select(i => i.Tag as Notice)
                        .Where(n => n != null && !n.IsClosing)
                        .ToList();

                    for (int i = 0; i <= open.Count - maxCount.Value; i++)
                    {
                        open[i]!.Dispose();
                    }
                }

                item = new MessageItem
                {
                    Type = type,
                    Content = content,
                    Tag = this,
                    Opacity = 0,
                    RenderTransform = new TranslateTransform()
                };

                host.Children.Add(item);
                dispatcher.BeginInvoke(new Action(AnimateIn), DispatcherPriority.Loaded);

                if (duration > TimeSpan.Zero)
                {
                    timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher) { Interval = duration };
                    timer.Tick += (s, e) => Dispose();
                    timer.Start();
                }
            }

            private void AnimateIn()
            {
                if (item == null || closing) return;

                var ease = new CircleEase { EasingMode = EasingMode.EaseOut };
                item.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-item.ActualHeight, 0, AnimationDuration) { EasingFunction = ease });
                item.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, AnimationDuration) { EasingFunction = ease });
            }

            private void Close()
            {
                timer?.Stop();

                if (item == null || closing) return;

                closing = true;

                var ease = new CircleEase { EasingMode = EasingMode.EaseInOut };
                var height = item.ActualHeight;
                var collapse = new DoubleAnimation(height, 0, AnimationDuration) { EasingFunction = ease };
                collapse.Completed += (s, e) => Remove();

                item.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-height, AnimationDuration) { EasingFunction = ease });
                item.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, AnimationDuration) { EasingFunction = ease });
                item.BeginAnimation(FrameworkElement.MarginProperty, new ThicknessAnimation(new Thickness(0), AnimationDuration) { EasingFunction = ease });
                item.BeginAnimation(FrameworkElement.HeightProperty, collapse);
            }

            private void Remove()
            {
                if (item == null || host == null || layer == null) return;

                host.Children.Remove(item);
                item = null;

                if (host.Children.Count == 0)
                {
                    layer.Root.Children.Remove(host);
                    hosts.Remove(layer);
                    layer.RemoveIfEmpty();
                }
            }
        }

        #endregion
    }
}
