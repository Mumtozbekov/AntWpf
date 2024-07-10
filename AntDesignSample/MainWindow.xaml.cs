using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AntWpf.Controls;

namespace AntDesignSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        ObservableCollection<AntIconKey> iconsCollection = new();
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            foreach (var animalValue in Enum.GetValues<AntIconKey>())
            {
                iconsCollection.Add(animalValue);
            }
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
