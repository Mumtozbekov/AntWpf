namespace AntWpf.Controls
{
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// In-window overlay used by Message (and Modal): one adorner per window, placed on the
    /// window content's adorner layer, hosting a Grid that covers the client area. It lives in
    /// the window's visual tree, so DynamicResource theme brushes and inherited properties work.
    /// </summary>
    internal sealed class OverlayLayer : Adorner
    {
        #region Fields

        private static readonly ConditionalWeakTable<Window, OverlayLayer> layers = new ConditionalWeakTable<Window, OverlayLayer>();

        private readonly Window window;

        private readonly Grid root = new Grid();

        #endregion

        #region Constructors

        private OverlayLayer(Window window, UIElement adornedElement) : base(adornedElement)
        {
            this.window = window;
            AddVisualChild(root);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the grid that hosts the overlay content. It has no background, so only its children are hit-testable.
        /// </summary>
        public Grid Root => root;

        protected override int VisualChildrenCount => 1;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the active window of the application, or its main window.
        /// </summary>
        public static Window? GetActiveWindow()
        {
            var application = Application.Current;

            if (application == null) return null;

            return application.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? application.MainWindow;
        }

        /// <summary>
        /// Gets (or creates) the overlay of <paramref name="window"/>. Returns null when the window
        /// content has no adorner layer.
        /// </summary>
        public static OverlayLayer? Get(Window window)
        {
            if (layers.TryGetValue(window, out var existing))
            {
                return existing;
            }

            if (!(window.Content is UIElement content) || !(AdornerLayer.GetAdornerLayer(content) is AdornerLayer adornerLayer))
            {
                Trace.TraceWarning("AntWpf: the window '{0}' has no adorner layer, the overlay cannot be shown.", window.Title);
                return null;
            }

            var layer = new OverlayLayer(window, content);
            adornerLayer.Add(layer);
            layers.Add(window, layer);
            return layer;
        }

        /// <summary>
        /// Removes the overlay from the window when it hosts nothing anymore.
        /// </summary>
        public void RemoveIfEmpty()
        {
            if (root.Children.Count > 0) return;

            AdornerLayer.GetAdornerLayer(AdornedElement)?.Remove(this);
            layers.Remove(window);
        }

        #endregion

        #region Overrides

        protected override Visual GetVisualChild(int index)
        {
            return root;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var size = AdornedElement.RenderSize;
            root.Measure(size);
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            root.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}
