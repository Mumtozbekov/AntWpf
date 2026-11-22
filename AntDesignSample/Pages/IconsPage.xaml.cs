using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

using AntWpf.Controls;

namespace AntDesignSample.Pages
{
    public partial class IconsPage : UserControl
    {
        private readonly ObservableCollection<AntIconKey> iconsCollection = new(Enum.GetValues<AntIconKey>());

        public IconsPage()
        {
            InitializeComponent();
            iconsList.ItemsSource = iconsCollection;
        }

        private void iconSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var key = iconSearchBox.Text;

            var view = CollectionViewSource.GetDefaultView(iconsCollection);

            view.Filter = (o) =>
            {
                if (string.IsNullOrEmpty(key))
                    return true;

                if (o is AntIconKey iconKey)
                {
                    return iconKey.ToString().ToLower().Contains(key.ToLower());
                }
                return false;
            };
        }
    }
}
