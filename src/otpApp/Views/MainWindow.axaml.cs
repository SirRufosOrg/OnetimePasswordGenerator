using Avalonia.Controls;
using otpApp.ViewModels;

namespace otpApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.ShowAddDialog) && vm.ShowAddDialog)
                {
                    ContentScrollViewer?.ScrollToEnd();
                }
            };
        }
    }
}
