using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using otpApp.Models;
using otpApp.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Zafiro.UI.Commands;

namespace otpApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly AccountRepository _repository;
    private readonly TotpService _totpService;
    private readonly SourceList<AccountItemViewModel> _accountsSource = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly ReadOnlyObservableCollection<AccountItemViewModel> _accounts;

    [Reactive] private bool _showAddDialog;

    public ReadOnlyObservableCollection<AccountItemViewModel> Accounts => _accounts;
    public IEnhancedCommand ShowAddCommand { get; }
    public AddAccountViewModel AddAccountViewModel { get; private set; }

    public MainWindowViewModel(
        AccountRepository repository,
        TotpService totpService,
        AddAccountViewModel addAccountViewModel)
    {
        _repository = repository;
        _totpService = totpService;
        AddAccountViewModel = addAccountViewModel;

        SubscribeToAddDialog(AddAccountViewModel);

        _accountsSource
            .Connect()
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Bind(out _accounts)
            .Subscribe()
            .DisposeWith(_disposables);

        ShowAddCommand = ReactiveCommand.Create(() => ShowAddDialog = true)
            .Enhance("Add Account", "ShowAdd");

        LoadAccounts();
    }

    private void SubscribeToAddDialog(AddAccountViewModel vm)
    {
        vm.Saved
            .RegisterHandler(OnAccountSaved)
            .DisposeWith(_disposables);

        vm.CancelledInteraction
            .RegisterHandler(ctx =>
            {
                ShowAddDialog = false;
                ctx.SetOutput(Unit.Default);
            })
            .DisposeWith(_disposables);
    }

    private async Task OnAccountSaved(IInteractionContext<OtpAccount, Unit> ctx)
    {
        var account = ctx.Input;
        _repository.Insert(account);
        ShowAddDialog = false;
        LoadAccounts();
        ResetAddDialog();
        ctx.SetOutput(Unit.Default);
    }

    private void LoadAccounts()
    {
        var accounts = _repository.GetAll();
        var accountVms = accounts.Select(a => new AccountItemViewModel(a, _totpService, DeleteAccount));
        _accountsSource.Edit(innerList =>
        {
            innerList.Clear();
            innerList.AddRange(accountVms);
        });
    }

    private void ResetAddDialog()
    {
        var newVm = new AddAccountViewModel();
        SubscribeToAddDialog(newVm);
        AddAccountViewModel = newVm;
        this.RaisePropertyChanged(nameof(AddAccountViewModel));
    }

    private void DeleteAccount(AccountItemViewModel item)
    {
        _repository.Delete(item.Account.Id);
        _accountsSource.Remove(item);
        item.Dispose();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        foreach (var account in _accountsSource.Items)
        {
            account.Dispose();
        }
    }
}
