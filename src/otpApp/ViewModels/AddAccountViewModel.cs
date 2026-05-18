namespace otpApp.ViewModels;

public partial class AddAccountViewModel : ViewModelBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    [Reactive] private string _issuer = "";
    [Reactive] private string _label = "";
    [Reactive] private string _secret = "";
    [Reactive] private OtpAlgorithm _algorithm = OtpAlgorithm.SHA1;
    [Reactive] private int _digits = 6;
    [Reactive] private int _period = 30;

    public OtpAlgorithm[] Algorithms => Enum.GetValues<OtpAlgorithm>();

    public IEnhancedCommand SaveCommand { get; }
    public IEnhancedCommand CancelCommand { get; }

    public Interaction<OtpAccount, Unit> Saved { get; } = new();
    public Interaction<Unit, Unit> CancelledInteraction { get; } = new();

    public AddAccountViewModel()
    {
        var canSave = this.WhenAnyValue(
            x => x.Issuer,
            x => x.Label,
            x => x.Secret,
            x => x.Digits,
            x => x.Period,
            (issuer, label, secret, digits, period) =>
                !string.IsNullOrWhiteSpace(issuer) &&
                !string.IsNullOrWhiteSpace(label) &&
                !string.IsNullOrWhiteSpace(secret) &&
                digits > 0 &&
                period > 0
        );

        SaveCommand = ReactiveCommand.Create(Save, canSave)
            .Enhance(Loc.CmdSave, "SaveAccount");

        CancelCommand = ReactiveCommand.CreateFromObservable(() =>
            CancelledInteraction.Handle(Unit.Default).Select(_ => Unit.Default))
            .Enhance(Loc.CmdCancel, "CancelAdd");
    }

    private void Save()
    {
        var account = new OtpAccount
        {
            Issuer = Issuer.Trim(),
            Label = Label.Trim(),
            SecretBase32 = Secret.Trim(),
            Algorithm = Algorithm,
            Digits = Digits,
            Period = Period,
            CreatedAt = DateTime.UtcNow
        };

        Saved.Handle(account).Subscribe();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
