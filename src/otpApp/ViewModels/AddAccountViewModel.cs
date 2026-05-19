namespace otpApp.ViewModels;

public partial class AddAccountViewModel : ViewModelBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    [Reactive] private string _issuer = "";
    [Reactive] private string _label = "";
    [Reactive] private string _secret = "";
    [Reactive] private OtpType _type = OtpType.Totp;
    [Reactive] private OtpAlgorithm _algorithm = OtpAlgorithm.SHA1;
    [Reactive] private int _digits = 6;
    [Reactive] private int _period = 30;
    [Reactive] private long _hotpCounter;

    [Reactive] private bool _isTotp = true;

    public OtpType[] OtpTypes => Enum.GetValues<OtpType>();
    public OtpAlgorithm[] Algorithms => Enum.GetValues<OtpAlgorithm>();

    public IEnhancedCommand SaveCommand { get; }
    public IEnhancedCommand CancelCommand { get; }

    public Interaction<OtpAccount, Unit> Saved { get; } = new();
    public Interaction<Unit, Unit> CancelledInteraction { get; } = new();

    public AddAccountViewModel()
    {
        this.WhenAnyValue(x => x.Type)
            .Subscribe(t => IsTotp = t == OtpType.Totp)
            .DisposeWith(_disposables);

        var canSave = this.WhenAnyValue(
            x => x.Issuer,
            x => x.Label,
            x => x.Secret,
            x => x.Digits,
            x => x.Period,
            x => x.Type,
            x => x.HotpCounter,
                (issuer, label, secret, digits, period, type, _) =>
                    !string.IsNullOrWhiteSpace(issuer) &&
                    !string.IsNullOrWhiteSpace(label) &&
                    !string.IsNullOrWhiteSpace(secret) &&
                    (digits == 6 || digits == 8) &&
                    (type == OtpType.Hotp || period > 0)
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
            Type = Type,
            Issuer = Issuer.Trim(),
            Label = Label.Trim(),
            SecretBase32 = Secret.Trim(),
            Algorithm = Algorithm,
            Digits = Digits,
            Period = Period,
            HotpCounter = HotpCounter,
            CreatedAt = DateTime.UtcNow
        };

        Saved.Handle(account).Subscribe();
    }

    public void Reset()
    {
        Issuer = "";
        Label = "";
        Secret = "";
        Type = OtpType.Totp;
        Algorithm = OtpAlgorithm.SHA1;
        Digits = 6;
        Period = 30;
        HotpCounter = 0;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
