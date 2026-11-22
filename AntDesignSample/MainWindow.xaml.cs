using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using AntDesignSample.Pages;
using AntWpf;

namespace AntDesignSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Dictionary<string, Func<UserControl>> pageFactories = new()
        {
            ["General"] = () => new GeneralPage(),
            ["Icons"] = () => new IconsPage(),
            ["Tag"] = () => new TagPage(),
            ["Badge"] = () => new BadgePage(),
            ["Alert"] = () => new AlertPage(),
        };

        private readonly Dictionary<string, UserControl> pages = new();

        public MainWindow()
        {
            InitializeComponent();
            navigation.SelectedIndex = 0;
        }

        private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (navigation.SelectedItem is not ListBoxItem { Tag: string key }) return;

            if (!pages.TryGetValue(key, out var view))
            {
                view = pageFactories[key]();
                pages[key] = view;
            }

            page.Content = view;
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.SetTheme(ThemeManager.Current == AntTheme.Light ? AntTheme.Dark : AntTheme.Light);
            themeButton.Content = ThemeManager.Current == AntTheme.Light ? "Dark" : "Light";
        }
    }
}
