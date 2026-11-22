using System;
using System.Windows;
using System.Windows.Controls;

namespace AntDesignSample.Pages
{
    public partial class BadgePage : UserControl
    {
        public BadgePage()
        {
            InitializeComponent();
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            dynamicBadge.Count++;
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            dynamicBadge.Count = Math.Max(0, dynamicBadge.Count - 1);
        }
    }
}
