using DynamicData;
using DynamicData.Binding;

namespace otpApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly AccountRepository _repository;
    private readonly TotpService _totpService;
    private readonly IClipboardService _clipboardService;
    private readonly IDialogService _dialogService;
    private readonly SourceList<AccountItemViewModel> _accountsSource = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly ReadOnlyObservableCollection<AccountItemViewModel> _accounts;

    [Reactive] private bool _showAddDialog;
    [Reactive] private int _selectedCultureIndex = LocalizationService.Default.CurrentCulture == "de" ? 1 : 0;

    public string[] Languages => ["English", "Deutsch"];
    public bool ShowAboutButton => !OperatingSystem.IsMacOS();

    public ReadOnlyObservableCollection<AccountItemViewModel> Accounts => _accounts;
    public IEnhancedCommand ShowAddCommand { get; }
    public IEnhancedCommand ShowAboutCommand { get; }
    public AddAccountViewModel AddAccountViewModel { get; private set; }

    public MainWindowViewModel(
        AccountRepository repository,
        TotpService totpService,
        AddAccountViewModel addAccountViewModel,
        IClipboardService clipboardService,
        IDialogService dialogService)
    {
        _repository = repository;
        _totpService = totpService;
        _clipboardService = clipboardService;
        _dialogService = dialogService;
        AddAccountViewModel = addAccountViewModel;

        SubscribeToAddDialog(AddAccountViewModel);

        _accountsSource
            .Connect()
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Bind(out _accounts)
            .Subscribe()
            .DisposeWith(_disposables);

        ShowAddCommand = ReactiveCommand.Create(() => ShowAddDialog = true)
            .Enhance(Loc.CmdAddAccount, "ShowAdd");

        ShowAboutCommand = ReactiveCommand.Create(_dialogService.ShowAbout)
            .Enhance(Loc.CmdAbout, "ShowAbout");

        this.WhenAnyValue(x => x.SelectedCultureIndex)
            .Subscribe(index => Loc.CurrentCulture = index == 0 ? "en" : "de")
            .DisposeWith(_disposables);

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
        var accountVms = accounts.Select(a => new AccountItemViewModel(a, _totpService, _clipboardService, DeleteAccount, EditAccount));
        _accountsSource.Edit(innerList =>
        {
            foreach (var vm in innerList)
            {
                vm.Dispose();
            }
            innerList.Clear();
            innerList.AddRange(accountVms);
        });
    }

    private void ResetAddDialog()
    {
        AddAccountViewModel.Reset();
    }

    private void DeleteAccount(AccountItemViewModel item)
    {
        _repository.Delete(item.Account.Id);
        _accountsSource.Remove(item);
        item.Dispose();
    }

    private void EditAccount(AccountItemViewModel item)
    {
        var account = item.Account;
        account.Issuer = item.EditIssuer.Trim();
        account.Label = item.EditLabel.Trim();
        account.SecretBase32 = item.EditSecret.Trim();
        account.Algorithm = item.EditAlgorithm;
        account.Digits = item.EditDigits;
        account.Period = item.EditPeriod;

        _repository.Update(account);
        item.NotifyAccountUpdated();
        item.IsEditing = false;
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
