namespace AntWpf.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Modal dialog shown as an overlay inside the current window. Declare it anywhere in XAML (it takes
    /// no space) and open it with <see cref="IsOpen"/>. The content keeps its DataContext and bindings.
    /// </summary>
    public class Modal : System.Windows.Controls.ContentControl
    {
        #region Fields

        private ModalDialog? dialog;

        private bool isUnloading;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the OK button is clicked. Set <see cref="CancelEventArgs.Cancel"/> to keep the modal open.
        /// </summary>
        public event EventHandler<CancelEventArgs>? Ok;

        /// <summary>
        /// Occurs when the modal is cancelled (Cancel button, close button, mask click or Esc).
        /// Set <see cref="CancelEventArgs.Cancel"/> to keep it open.
        /// </summary>
        public event EventHandler<CancelEventArgs>? Cancel;

        /// <summary>
        /// Occurs after the close animation has finished.
        /// </summary>
        public event EventHandler? AfterClose;

        #endregion

        #region Properties

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(Modal),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

        /// <summary>
        /// Gets/sets whether the modal is open. Binds two-way by default.
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(Modal), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the title.
        /// </summary>
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(object), typeof(Modal), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets a custom footer. When null the Cancel and OK buttons are shown.
        /// </summary>
        public object Footer
        {
            get { return GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty OkTextProperty =
            DependencyProperty.Register("OkText", typeof(string), typeof(Modal), new PropertyMetadata("OK"));

        /// <summary>
        /// Gets/sets the text of the OK button.
        /// </summary>
        public string OkText
        {
            get { return (string)GetValue(OkTextProperty); }
            set { SetValue(OkTextProperty, value); }
        }

        public static readonly DependencyProperty CancelTextProperty =
            DependencyProperty.Register("CancelText", typeof(string), typeof(Modal), new PropertyMetadata("Cancel"));

        /// <summary>
        /// Gets/sets the text of the Cancel button.
        /// </summary>
        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        public static readonly DependencyProperty OkButtonLoadingProperty =
            DependencyProperty.Register("OkButtonLoading", typeof(bool), typeof(Modal), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets the loading state of the OK button.
        /// </summary>
        public bool OkButtonLoading
        {
            get { return (bool)GetValue(OkButtonLoadingProperty); }
            set { SetValue(OkButtonLoadingProperty, value); }
        }

        public static readonly DependencyProperty ClosableProperty =
            DependencyProperty.Register("Closable", typeof(bool), typeof(Modal), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether the close (x) button is shown.
        /// </summary>
        public bool Closable
        {
            get { return (bool)GetValue(ClosableProperty); }
            set { SetValue(ClosableProperty, value); }
        }

        public static readonly DependencyProperty MaskClosableProperty =
            DependencyProperty.Register("MaskClosable", typeof(bool), typeof(Modal), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether clicking the mask cancels the modal.
        /// </summary>
        public bool MaskClosable
        {
            get { return (bool)GetValue(MaskClosableProperty); }
            set { SetValue(MaskClosableProperty, value); }
        }

        public static readonly DependencyProperty KeyboardProperty =
            DependencyProperty.Register("Keyboard", typeof(bool), typeof(Modal), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets whether Esc cancels the modal.
        /// </summary>
        public bool Keyboard
        {
            get { return (bool)GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        public static readonly DependencyProperty CenteredProperty =
            DependencyProperty.Register("Centered", typeof(bool), typeof(Modal), new PropertyMetadata(false));

        /// <summary>
        /// Gets/sets whether the dialog is vertically centered (otherwise 100px from the top).
        /// </summary>
        public bool Centered
        {
            get { return (bool)GetValue(CenteredProperty); }
            set { SetValue(CenteredProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var modal = (Modal)d;

            if ((bool)e.NewValue)
            {
                // Before the modal is loaded (e.g. IsOpen="True" in XAML) it opens on Loaded.
                if (modal.IsLoaded)
                {
                    modal.Open();
                }
            }
            else
            {
                modal.dialog?.Close();
            }
        }

        #endregion

        #region Constructors

        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal)));
        }

        public Modal()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows a confirm dialog with OK and Cancel. Returns true for OK, false for Cancel or Esc.
        /// </summary>
        public static Task<bool> Confirm(object title, object? content = null, string? okText = null, string? cancelText = null)
            => ShowStatic(ModalType.Confirm, title, content, okText, cancelText, showCancel: true);

        /// <summary>
        /// Shows an information dialog with an OK button. Returns true for OK, false for Esc.
        /// </summary>
        public static Task<bool> Info(object title, object? content = null, string? okText = null)
            => ShowStatic(ModalType.Info, title, content, okText, null, showCancel: false);

        /// <summary>
        /// Shows a success dialog with an OK button. Returns true for OK, false for Esc.
        /// </summary>
        public static Task<bool> Success(object title, object? content = null, string? okText = null)
            => ShowStatic(ModalType.Success, title, content, okText, null, showCancel: false);

        /// <summary>
        /// Shows an error dialog with an OK button. Returns true for OK, false for Esc.
        /// </summary>
        public static Task<bool> Error(object title, object? content = null, string? okText = null)
            => ShowStatic(ModalType.Error, title, content, okText, null, showCancel: false);

        /// <summary>
        /// Shows a warning dialog with an OK button. Returns true for OK, false for Esc.
        /// </summary>
        public static Task<bool> Warning(object title, object? content = null, string? okText = null)
            => ShowStatic(ModalType.Warning, title, content, okText, null, showCancel: false);

        #endregion

        #region Private Methods

        private void Open()
        {
            if (dialog != null)
            {
                // Reopened while the previous dialog is still closing.
                dialog.RemoveNow();
            }

            var window = Window.GetWindow(this) ?? OverlayLayer.GetActiveWindow();

            if (window == null)
            {
                Trace.TraceWarning("AntWpf: no window to show the modal in.");
                SetCurrentValue(IsOpenProperty, false);
                return;
            }

            var newDialog = new ModalDialog();
            Bind(newDialog, ModalDialog.TitleProperty, TitleProperty);
            Bind(newDialog, ModalDialog.BodyProperty, ContentProperty);
            Bind(newDialog, ModalDialog.BodyTemplateProperty, ContentTemplateProperty);
            Bind(newDialog, ModalDialog.FooterProperty, FooterProperty);
            Bind(newDialog, ModalDialog.OkTextProperty, OkTextProperty);
            Bind(newDialog, ModalDialog.CancelTextProperty, CancelTextProperty);
            Bind(newDialog, ModalDialog.OkButtonLoadingProperty, OkButtonLoadingProperty);
            Bind(newDialog, ModalDialog.ClosableProperty, ClosableProperty);
            Bind(newDialog, ModalDialog.MaskClosableProperty, MaskClosableProperty);
            Bind(newDialog, ModalDialog.KeyboardProperty, KeyboardProperty);
            Bind(newDialog, ModalDialog.CenteredProperty, CenteredProperty);
            Bind(newDialog, ModalDialog.DialogWidthProperty, WidthProperty);
            Bind(newDialog, DataContextProperty, DataContextProperty);

            newDialog.OkRequested += OnOkRequested;
            newDialog.CancelRequested += OnCancelRequested;
            newDialog.Closed += OnDialogClosed;

            dialog = newDialog;

            if (!newDialog.Show(window))
            {
                dialog = null;
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void Bind(ModalDialog target, DependencyProperty targetProperty, DependencyProperty sourceProperty)
        {
            target.SetBinding(targetProperty, new Binding { Source = this, Path = new PropertyPath(sourceProperty), Mode = BindingMode.OneWay });
        }

        private void OnOkRequested(object? sender, EventArgs e)
        {
            var args = new CancelEventArgs();
            Ok?.Invoke(this, args);

            if (!args.Cancel)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void OnCancelRequested(object? sender, EventArgs e)
        {
            var args = new CancelEventArgs();
            Cancel?.Invoke(this, args);

            if (!args.Cancel)
            {
                SetCurrentValue(IsOpenProperty, false);
            }
        }

        private void OnDialogClosed(object? sender, EventArgs e)
        {
            if (ReferenceEquals(sender, dialog))
            {
                dialog = null;
            }

            if (sender is ModalDialog closed)
            {
                BindingOperations.ClearAllBindings(closed);
            }

            // Hidden because the modal was unloaded, not closed: IsOpen is unchanged.
            if (!isUnloading)
            {
                AfterClose?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsOpen && dialog == null)
            {
                Open();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // The page holding the modal went away: do not leave the dialog in the window. IsOpen is kept,
            // so the dialog comes back when the modal is loaded again and IsOpen is still true.
            if (dialog != null)
            {
                isUnloading = true;
                dialog.RemoveNow();
                isUnloading = false;
            }
        }

        private static Task<bool> ShowStatic(ModalType type, object title, object? content, string? okText, string? cancelText, bool showCancel)
        {
            var dispatcher = Application.Current?.Dispatcher
                ?? throw new InvalidOperationException("Modal requires an Application instance.");

            if (!dispatcher.CheckAccess())
            {
                return dispatcher.InvokeAsync(() => ShowStatic(type, title, content, okText, cancelText, showCancel)).Task.Unwrap();
            }

            var completion = new TaskCompletionSource<bool>();
            var window = OverlayLayer.GetActiveWindow();

            if (window == null)
            {
                Trace.TraceWarning("AntWpf: no window to show the modal in.");
                completion.SetResult(false);
                return completion.Task;
            }

            var result = false;
            var dialog = new ModalDialog
            {
                Type = type,
                Title = title,
                Body = content,
                ShowCancel = showCancel,
                Closable = false,
                MaskClosable = false,
                DialogWidth = window.TryFindResource("ModalConfirmWidth") is double width ? width : 416
            };

            if (okText != null) dialog.OkText = okText;
            if (cancelText != null) dialog.CancelText = cancelText;

            dialog.OkRequested += (s, e) => { result = true; dialog.Close(); };
            dialog.CancelRequested += (s, e) => { result = false; dialog.Close(); };
            dialog.Closed += (s, e) => completion.TrySetResult(result);

            if (!dialog.Show(window))
            {
                completion.TrySetResult(false);
            }

            return completion.Task;
        }

        #endregion
    }
}
