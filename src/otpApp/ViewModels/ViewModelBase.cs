namespace otpApp.ViewModels;

public partial class ViewModelBase : ReactiveObject
{
    public LocalizationService Loc { get; } = LocalizationService.Default;
}
