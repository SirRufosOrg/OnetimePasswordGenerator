using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using otpApp.ViewModels;

namespace otpApp.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        DataContextChanged += (_, _) =>
        {
            if (DataContext is AboutWindowViewModel vm)
            {
                vm.RequestClose += () => Close();
            }
        };
    }
}