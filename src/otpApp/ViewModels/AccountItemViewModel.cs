namespace otpApp.ViewModels;

public partial class AccountItemViewModel : ViewModelBase, IDisposable
{
    private readonly ITotpService _totpService;
    private readonly IClipboardService _clipboardService;
    private readonly CompositeDisposable _disposables = new();
    private readonly CompositeDisposable _timerDisposables = new();
    internal CompositeDisposable Disposables => _disposables;

    [Reactive] private OtpAccount _account = default!;
    [Reactive] private string _displayIssuer = "";
    [Reactive] private string _displayLabel = "";
    [Reactive] private string _currentCode = "";
    [Reactive] private int _remainingSeconds;
    [Reactive] private double _progress;
    [Reactive] private string _progressColorClass = "";
    [Reactive] private long _counter;
    [Reactive] private bool _isTotp = true;

    [Reactive] private bool _isEditing;
    [Reactive] private bool _isEditTotp = true;
    [Reactive] private string _editIssuer = "";
    [Reactive] private string _editLabel = "";
    [Reactive] private string _editSecret = "";
    [Reactive] private OtpType _editType;
    [Reactive] private OtpAlgorithm _editAlgorithm;
    [Reactive] private int _editDigits;
    [Reactive] private int _editPeriod;
    [Reactive] private long _editCounter;

    public OtpType[] OtpTypes => Enum.GetValues<OtpType>();
    public OtpAlgorithm[] Algorithms => Enum.GetValues<OtpAlgorithm>();
    public IEnhancedCommand CopyCommand { get; }
    public IEnhancedCommand DeleteCommand { get; }
    public IEnhancedCommand EditCommand { get; }
    public IEnhancedCommand SaveEditCommand { get; }
    public IEnhancedCommand CancelEditCommand { get; }
    public IEnhancedCommand NextCodeCommand { get; }

    public Interaction<Unit, Unit> DeleteRequested { get; } = new();
    public Interaction<Unit, Unit> EditRequested { get; } = new();
    public Interaction<Unit, Unit> CounterAdvancedRequested { get; } = new();

    public AccountItemViewModel(
        OtpAccount account,
        ITotpService totpService,
        IClipboardService clipboardService,
        LocalizationService localizationService)
        : base(localizationService)
    {
        _account = account;
        _displayIssuer = account.Issuer;
        _displayLabel = account.Label;
        _counter = account.HotpCounter;
        _isTotp = account.Type == OtpType.Totp;
        _totpService = totpService;
        _clipboardService = clipboardService;

        CopyCommand = ReactiveCommand.CreateFromTask(CopyCode)
            .Enhance(Loc.CmdCopy, "CopyCode");

        CopyCommand.ThrownExceptions
            .Subscribe(ex => Console.Error.WriteLine($"Copy failed: {ex}"))
            .DisposeWith(_disposables);

        DeleteCommand = ReactiveCommand.CreateFromObservable(() =>
                DeleteRequested.Handle(Unit.Default))
            .Enhance(Loc.CmdDelete, "DeleteAccount");

        EditCommand = ReactiveCommand.Create(StartEdit)
            .Enhance(Loc.CmdEdit, "EditAccount");

        var canSave = this.WhenAnyValue(x => x.EditDigits, d => d is >= 6 and <= 9);
        SaveEditCommand = ReactiveCommand.CreateFromObservable(() =>
                EditRequested.Handle(Unit.Default), canSave)
            .Enhance(Loc.CmdSave, "SaveAccountEdit");

        CancelEditCommand = ReactiveCommand.Create(() => IsEditing = false)
            .Enhance(Loc.CmdCancel, "CancelEdit");

        NextCodeCommand = ReactiveCommand.Create(AdvanceCounter)
            .Enhance(Loc.NextCode, "NextCode");

        this.WhenAnyValue(x => x.EditType)
            .Subscribe(t => IsEditTotp = t == OtpType.Totp)
            .DisposeWith(_disposables);

        GenerateInitialCode();

        if (IsTotp)
        {
            StartTimer();
        }
    }

    private async Task CopyCode()
    {
        await _clipboardService.CopyToClipboardAsync(CurrentCode);
    }

    private void StartEdit()
    {
        EditIssuer = Account.Issuer;
        EditLabel = Account.Label;
        EditSecret = Account.SecretBase32;
        EditType = Account.Type;
        EditAlgorithm = Account.Algorithm;
        EditDigits = Account.Digits;
        EditPeriod = Account.Period;
        EditCounter = Account.HotpCounter;
        IsEditing = true;
    }

    private void AdvanceCounter()
    {
        Counter++;
        Account.HotpCounter = Counter;
        CurrentCode = _totpService.GenerateCode(Account, Counter);
        CounterAdvancedRequested.Handle(Unit.Default).Subscribe();
    }

    private void GenerateInitialCode()
    {
        if (IsTotp)
        {
            CurrentCode = _totpService.GenerateCode(Account);
        }
        else
        {
            CurrentCode = _totpService.GenerateCode(Account, Counter);
        }
    }

    private void StartTimer()
    {
        _timerDisposables.Clear();

        Observable.Interval(TimeSpan.FromSeconds(1))
            .StartWith(0)
            .Select(_ => _totpService.RemainingSeconds(Account))
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateRemaining)
            .DisposeWith(_timerDisposables);

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
            .DisposeWith(_timerDisposables);
    }

    private void UpdateRemaining(int seconds)
    {
        RemainingSeconds = seconds;
        if (seconds >= Account.Period || seconds <= 0)
        {
            CurrentCode = _totpService.GenerateCode(Account);
        }
    }

    public void RefreshAfterEdit()
    {
        DisplayIssuer = Account.Issuer;
        DisplayLabel = Account.Label;
        IsTotp = Account.Type == OtpType.Totp;

        if (IsTotp)
        {
            CurrentCode = _totpService.GenerateCode(Account);
            StartTimer();
        }
        else
        {
            _timerDisposables.Clear();
            Counter = Account.HotpCounter;
            CurrentCode = _totpService.GenerateCode(Account, Counter);
        }
    }

    public void Dispose()
    {
        _timerDisposables.Dispose();
        _disposables.Dispose();
    }
}
