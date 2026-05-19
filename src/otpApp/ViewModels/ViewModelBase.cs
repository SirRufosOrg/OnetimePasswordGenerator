namespace otpApp.ViewModels;

public partial class ViewModelBase : ReactiveObject
{
    public LocalizationService Loc { get; }

    public ViewModelBase(LocalizationService loc)
    {
        Loc = loc;
    }

#if DEBUG
    public ViewModelBase() : this(new LocalizationService()) { }
#endif
}