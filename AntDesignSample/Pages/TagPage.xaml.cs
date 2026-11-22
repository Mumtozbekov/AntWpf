using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace AntDesignSample.Pages
{
    public partial class TagPage : UserControl
    {
        public TagPage()
        {
            InitializeComponent();
        }

        private void PreventClose_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            eventLog.Text = "Closing was cancelled.";
        }

        private void Tag_Closed(object? sender, EventArgs e)
        {
            eventLog.Text = "Tag closed.";
        }
    }
}
