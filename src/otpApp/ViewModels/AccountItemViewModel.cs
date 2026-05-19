namespace otpApp.ViewModels;

public partial class AccountItemViewModel : ViewModelBase, IDisposable
{
    private readonly TotpService _totpService;
    private readonly IClipboardService _clipboardService;
    private readonly CompositeDisposable _disposables = new();

    [Reactive] private OtpAccount _account = default!;
    [Reactive] private string _displayIssuer = "";
    [Reactive] private string _displayLabel = "";
    [Reactive] private string _currentCode = "";
    [Reactive] private int _remainingSeconds;
    [Reactive] private double _progress;
    [Reactive] private string _progressColorClass = "";

    [Reactive] private bool _isEditing;
    [Reactive] private string _editIssuer = "";
    [Reactive] private string _editLabel = "";
    [Reactive] private string _editSecret = "";
    [Reactive] private OtpAlgorithm _editAlgorithm;
    [Reactive] private int _editDigits;
    [Reactive] private int _editPeriod;

    public OtpAlgorithm[] Algorithms => Enum.GetValues<OtpAlgorithm>();
    public IEnhancedCommand CopyCommand { get; }
    public IEnhancedCommand DeleteCommand { get; }
    public IEnhancedCommand EditCommand { get; }
    public IEnhancedCommand SaveEditCommand { get; }
    public IEnhancedCommand CancelEditCommand { get; }

    public AccountItemViewModel(
        OtpAccount account,
        TotpService totpService,
        IClipboardService clipboardService,
        Action<AccountItemViewModel> onDelete,
        Action<AccountItemViewModel> onEdit)
    {
        _account = account;
        _displayIssuer = account.Issuer;
        _displayLabel = account.Label;
        _totpService = totpService;
        _clipboardService = clipboardService;

        CopyCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await _clipboardService.CopyToClipboardAsync(CurrentCode);
            })
            .Enhance(Loc.CmdCopy, "CopyCode");

        CopyCommand.ThrownExceptions
            .Subscribe(ex => Console.Error.WriteLine($"Copy failed: {ex}"))
            .DisposeWith(_disposables);

        DeleteCommand = ReactiveCommand.Create(() => onDelete(this))
            .Enhance(Loc.CmdDelete, "DeleteAccount");

        EditCommand = ReactiveCommand.Create(() =>
        {
            EditIssuer = Account.Issuer;
            EditLabel = Account.Label;
            EditSecret = Account.SecretBase32;
            EditAlgorithm = Account.Algorithm;
            EditDigits = Account.Digits;
            EditPeriod = Account.Period;
            IsEditing = true;
        })
            .Enhance(Loc.CmdEdit, "EditAccount");

        SaveEditCommand = ReactiveCommand.Create(() => onEdit(this))
            .Enhance(Loc.CmdSave, "SaveAccountEdit");

        CancelEditCommand = ReactiveCommand.Create(() => IsEditing = false)
            .Enhance(Loc.CmdCancel, "CancelEdit");

        CurrentCode = _totpService.GenerateCode(Account);

        Observable.Interval(TimeSpan.FromSeconds(1))
            .StartWith(0)
            .Select(_ => _totpService.RemainingSeconds(Account))
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateRemaining)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.RemainingSeconds)
            .Subscribe(r =>
            {
                var p = Account.Period;
                Progress = p > 0 ? (double)r / p : 0;

                ProgressColorClass = r switch
                {
                    <= 5 => "Expiring",
                    <= 10 => "ExpiringSoon",
                    _ => ""
                };
            })
            .DisposeWith(_disposables);
    }

    private void UpdateRemaining(int seconds)
    {
        RemainingSeconds = seconds;
        if (seconds >= Account.Period || seconds <= 0)
        {
            CurrentCode = _totpService.GenerateCode(Account);
        }
    }

    public void NotifyAccountUpdated()
    {
        DisplayIssuer = Account.Issuer;
        DisplayLabel = Account.Label;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
