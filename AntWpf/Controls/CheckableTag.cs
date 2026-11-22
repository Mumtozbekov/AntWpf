namespace AntWpf.Controls
{
    using System.Windows;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Tag that toggles between checked and unchecked, like a checkbox.
    /// </summary>
    public class CheckableTag : ToggleButton
    {
        #region Constructors

        static CheckableTag()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckableTag), new FrameworkPropertyMetadata(typeof(CheckableTag)));
        }

        #endregion
    }
}
