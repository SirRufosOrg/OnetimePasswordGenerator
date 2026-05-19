using Avalonia.Controls;

namespace otpApp.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string message, string confirmText, string cancelText) : this()
    {
        InitializeComponent();

        MessageText.Text = message;
        CancelBtn.Content = cancelText;
        CancelBtn.Click += (_, _) => Close(false);
        ConfirmBtn.Content = confirmText;
        ConfirmBtn.Click += (_, _) => Close(true);
    }
}