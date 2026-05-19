using System.Reflection;

namespace otpApp.ViewModels;

public partial class AboutWindowViewModel : ViewModelBase
{
    public string Version { get; }
    public IEnhancedCommand CloseCommand { get; }

    public AboutWindowViewModel(LocalizationService localizationService)
        : base(localizationService)
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        Version = v is not null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "v1.0.0";

        CloseCommand = ReactiveCommand.Create(Close)
            .Enhance(Loc.CmdCancel, "CloseAbout");
    }

    public event Action? RequestClose;

    private void Close()
    {
        RequestClose?.Invoke();
    }
}