using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using AntWpf.Controls;

namespace AntDesignSample.Pages
{
    public partial class ModalPage : UserControl
    {
        public ModalPage()
        {
            InitializeComponent();
        }

        private void OpenBasic_Click(object sender, RoutedEventArgs e) => basicModal.IsOpen = true;

        private void BasicModal_Ok(object? sender, CancelEventArgs e)
        {
            eventLog.Text = "OK, name: " + nameBox.Text;
        }

        private void BasicModal_AfterClose(object? sender, EventArgs e)
        {
            Message.Info("Basic modal closed");
        }

        private void OpenAsync_Click(object sender, RoutedEventArgs e)
        {
            asyncText.Text = "The modal will be closed after two seconds when OK is clicked.";
            asyncModal.IsOpen = true;
        }

        private async void AsyncModal_Ok(object? sender, CancelEventArgs e)
        {
            // Keep it open while the work runs, then close it.
            e.Cancel = true;
            asyncModal.OkButtonLoading = true;
            asyncText.Text = "Working...";
            await Task.Delay(2000);
            asyncModal.OkButtonLoading = false;
            asyncModal.IsOpen = false;
        }

        private void OpenCustom_Click(object sender, RoutedEventArgs e) => customModal.IsOpen = true;

        private void CloseCustom_Click(object sender, RoutedEventArgs e) => customModal.IsOpen = false;

        private void OpenStrict_Click(object sender, RoutedEventArgs e) => strictModal.IsOpen = true;

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var ok = await Modal.Confirm("Do you want to delete these items?", "When clicked the OK button, the items are deleted.");
            eventLog.Text = "Confirm result: " + ok;
        }

        private async void Info_Click(object sender, RoutedEventArgs e)
        {
            eventLog.Text = "Info result: " + await Modal.Info("This is a notification message", "some messages...some messages...");
        }

        private async void Success_Click(object sender, RoutedEventArgs e)
        {
            eventLog.Text = "Success result: " + await Modal.Success("This is a success message", "some messages...some messages...");
        }

        private async void Error_Click(object sender, RoutedEventArgs e)
        {
            eventLog.Text = "Error result: " + await Modal.Error("This is an error message", "some messages...some messages...");
        }

        private async void Warning_Click(object sender, RoutedEventArgs e)
        {
            eventLog.Text = "Warning result: " + await Modal.Warning("This is a warning message", "some messages...some messages...");
        }
    }
}
