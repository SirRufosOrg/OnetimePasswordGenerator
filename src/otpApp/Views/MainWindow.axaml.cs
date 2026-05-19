using Avalonia.Controls;
using otpApp.ViewModels;

namespace otpApp.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _previousViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_previousViewModel is not null)
        {
            _previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (DataContext is MainWindowViewModel vm)
        {
            _previousViewModel = vm;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.ShowAddDialog) && DataContext is MainWindowViewModel vm && vm.ShowAddDialog)
        {
            ContentScrollViewer?.ScrollToEnd();
        }
    }
}