using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using otpApp.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Commands;

namespace otpApp.ViewModels;

public partial class AddAccountViewModel : ViewModelBase, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    [Reactive] private string _issuer = "";
    [Reactive] private string _label = "";
    [Reactive] private string _secret = "";

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
            (issuer, label, secret) =>
                !string.IsNullOrWhiteSpace(issuer) &&
                !string.IsNullOrWhiteSpace(label) &&
                !string.IsNullOrWhiteSpace(secret)
        );

        SaveCommand = ReactiveCommand.Create(Save, canSave)
            .Enhance("Save", "SaveAccount");

        CancelCommand = ReactiveCommand.CreateFromObservable(() =>
            CancelledInteraction.Handle(Unit.Default).Select(_ => Unit.Default))
            .Enhance("Cancel", "CancelAdd");
    }

    private void Save()
    {
        var account = new OtpAccount
        {
            Issuer = Issuer.Trim(),
            Label = Label.Trim(),
            SecretBase32 = Secret.Trim(),
            Algorithm = OtpAlgorithm.SHA1,
            Digits = 6,
            Period = 30,
            CreatedAt = DateTime.UtcNow
        };

        Saved.Handle(account).Subscribe();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
