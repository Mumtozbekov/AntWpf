using System;
using System.Windows;
using System.Windows.Controls;

using AntWpf.Controls;

namespace AntDesignSample.Pages
{
    public partial class TabsPage : UserControl
    {
        private int newTabIndex = 3;

        public TabsPage()
        {
            InitializeComponent();
        }

        private void Placement_Checked(object sender, RoutedEventArgs e)
        {
            if (positionTabs == null) return;

            positionTabs.TabStripPlacement = (Dock)Enum.Parse(typeof(Dock), (string)((RadioButton)sender).Content);
        }

        private void Size_Checked(object sender, RoutedEventArgs e)
        {
            if (positionTabs == null) return;

            switch ((string)((RadioButton)sender).Content)
            {
                case "Small": Tabs.SetSize(positionTabs, Sizes.Small); break;
                case "Large": Tabs.SetSize(positionTabs, Sizes.Large); break;
                default: Tabs.SetSize(positionTabs, null); break;
            }
        }

        private void AddTab_Click(object sender, RoutedEventArgs e)
        {
            var item = new TabItem { Header = "New Tab " + newTabIndex, Content = new TextBlock { Text = "Content of new Tab " + newTabIndex } };
            newTabIndex++;
            editableTabs.Items.Add(item);
            editableTabs.SelectedItem = item;
        }

        private void EditableTabs_CloseRequested(object sender, RoutedEventArgs e)
        {
            editableTabs.Items.Remove(e.OriginalSource);
        }
    }
}
