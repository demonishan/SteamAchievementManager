using System.Windows;
namespace SLAM.Reborn {
  public partial class WarningWindow : Window {
    public bool IsConfirmed { get; private set; }
    public WarningWindow(string message, string confirmText = "Okay", string cancelText = "Nope!", string title = "Warning", bool requireCurrentStats = false) {
      InitializeComponent();
      TitleText.Text = title;
      MessageText.Text = message;
      ConfirmButton.Content = confirmText;
      if (string.IsNullOrEmpty(cancelText)) CancelButton.Visibility = Visibility.Collapsed;
      else CancelButton.Content = cancelText;
      if (requireCurrentStats) {
        ConfirmButton.IsEnabled = false;
        ConfirmButton.Opacity = 0.5;
        ConfirmCheckBox.Visibility = Visibility.Visible;
      }
      Owner = Application.Current.MainWindow;
    }
    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
      ConfirmButton.IsEnabled = true;
      ConfirmButton.Opacity = 1;
    }
    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
      ConfirmButton.IsEnabled = false;
      ConfirmButton.Opacity = 0.5;
    }
    private void Close_Click(object sender, RoutedEventArgs e) {
      IsConfirmed = false;
      Close();
    }
    private void Confirm_Click(object sender, RoutedEventArgs e) {
      IsConfirmed = true;
      Close();
    }
  }
}