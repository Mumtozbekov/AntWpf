using System;
using System.Windows;
using System.Windows.Controls;

using AntWpf.Controls;

namespace AntDesignSample.Pages
{
    public partial class AlertPage : UserControl
    {
        public AlertPage()
        {
            InitializeComponent();
        }

        private void Alert_Closed(object? sender, EventArgs e)
        {
            eventLog.Text = "Closed: " + ((Alert)sender!).Message;
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in closableAlerts.Children)
            {
                if (child is Alert alert)
                {
                    alert.Visibility = Visibility.Visible;
                }
            }

            eventLog.Text = string.Empty;
        }
    }
}
