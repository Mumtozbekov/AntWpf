using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using AntWpf.Controls;

namespace AntDesignSample.Pages
{
    public partial class MessagePage : UserControl
    {
        private IDisposable? last;

        public MessagePage()
        {
            InitializeComponent();
        }

        private void Info_Click(object sender, RoutedEventArgs e) => Message.Info("This is a normal message");

        private void Success_Click(object sender, RoutedEventArgs e) => Message.Success("This is a success message");

        private void Warning_Click(object sender, RoutedEventArgs e) => Message.Warning("This is a warning message");

        private void Error_Click(object sender, RoutedEventArgs e) => Message.Error("This is an error message");

        private void Loading_Click(object sender, RoutedEventArgs e) => Message.Loading("Action in progress..");

        private async void LoadingThenSuccess_Click(object sender, RoutedEventArgs e)
        {
            using (Message.Loading("Saving...", TimeSpan.Zero))
            {
                await Task.Delay(2500);
            }

            Message.Success("Saved");
        }

        private void Long_Click(object sender, RoutedEventArgs e)
        {
            last = Message.Info("This message closes after 10 seconds", TimeSpan.FromSeconds(10));
        }

        private void CloseLast_Click(object sender, RoutedEventArgs e)
        {
            last?.Dispose();
            last = null;
        }

        private void Background_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Message.Success("Shown from thread " + Environment.CurrentManagedThreadId));
        }

        private void MaxCount_Click(object sender, RoutedEventArgs e)
        {
            Message.Config(maxCount: 3);
            Message.Info("At most 3 messages now");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Message.Config(top: 8, duration: TimeSpan.FromSeconds(3), maxCount: 0);
            Message.Info("Config reset");
        }
    }
}
