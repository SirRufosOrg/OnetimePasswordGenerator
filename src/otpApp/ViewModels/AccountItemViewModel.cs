using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using otpApp.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Commands;

namespace otpApp.ViewModels;

public partial class AccountItemViewModel : ViewModelBase, IDisposable
{
    private readonly TotpService _totpService;
    private readonly CompositeDisposable _disposables = new();

    [Reactive] private string _currentCode = "";
    [Reactive] private int _remainingSeconds;
    [Reactive] private double _progress;
    [Reactive] private string _progressColorClass = "";

    public OtpAccount Account { get; }
    public string DisplayName => $"{Account.Issuer} - {Account.Label}";
    public IEnhancedCommand CopyCommand { get; }
    public IEnhancedCommand DeleteCommand { get; }

    public AccountItemViewModel(
        OtpAccount account,
        TotpService totpService,
        Action<AccountItemViewModel> onDelete)
    {
        Account = account;
        _totpService = totpService;

        CopyCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var clipboard = GetClipboard();
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(CurrentCode);
                }
            }
            catch
            {
            }
        })
            .Enhance("Copy", "CopyCode");

        DeleteCommand = ReactiveCommand.Create(() => onDelete(this))
            .Enhance("Delete", "DeleteAccount");

        CurrentCode = _totpService.GenerateCode(Account);

        Observable.Interval(TimeSpan.FromSeconds(1))
            .StartWith(0)
            .Select(_ => _totpService.RemainingSeconds(Account))
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateRemaining)
            .DisposeWith(_disposables);

        this.WhenAnyValue(x => x.RemainingSeconds)
            .Subscribe(_ =>
            {
                var r = RemainingSeconds;
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

    private static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }
        return null;
    }

    private void UpdateRemaining(int seconds)
    {
        RemainingSeconds = seconds;
        if (seconds >= Account.Period || seconds <= 0)
        {
            CurrentCode = _totpService.GenerateCode(Account);
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
