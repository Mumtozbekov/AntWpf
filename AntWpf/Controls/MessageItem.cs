namespace AntWpf.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// A single notice shown by <see cref="Message"/>. Styled in Themes/Message.xaml.
    /// </summary>
    [TemplatePart(Name = PART_Icon, Type = typeof(AntIcon))]
    public class MessageItem : ContentControl
    {
        #region Fields

        private const string PART_Icon = "PART_Icon";

        private AntIcon? icon;

        #endregion

        #region Properties

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(MessageType), typeof(MessageItem), new PropertyMetadata(MessageType.Info, OnTypeChanged));

        /// <summary>
        /// Gets/sets the type of the message, which selects its icon.
        /// </summary>
        public MessageType Type
        {
            get { return (MessageType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MessageItem)d).UpdateIcon();
        }

        #endregion

        #region Constructors

        static MessageItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageItem), new FrameworkPropertyMetadata(typeof(MessageItem)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            icon = GetTemplateChild(PART_Icon) as AntIcon;
            UpdateIcon();
        }

        #endregion

        #region Private Methods

        private void UpdateIcon()
        {
            if (icon == null) return;

            switch (Type)
            {
                case MessageType.Success: icon.Key = AntIconKey.CheckCircleFilled; break;
                case MessageType.Warning: icon.Key = AntIconKey.ExclamationCircleFilled; break;
                case MessageType.Error: icon.Key = AntIconKey.CloseCircleFilled; break;
                case MessageType.Loading: icon.Key = AntIconKey.LoadingOutlined; break;
                default: icon.Key = AntIconKey.InfoCircleFilled; break;
            }
        }

        #endregion
    }

    public enum MessageType : byte
    {
        Info, Success, Warning, Error, Loading
    }
}
