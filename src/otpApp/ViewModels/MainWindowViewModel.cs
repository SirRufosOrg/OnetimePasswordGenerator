using DynamicData;
using DynamicData.Binding;

namespace otpApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly AccountRepository _repository;
    private readonly TotpService _totpService;
    private readonly IClipboardService _clipboardService;
    private readonly IDialogService _dialogService;
    private readonly IFileDialogService _fileDialogService;
    private readonly SourceList<AccountItemViewModel> _accountsSource = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly ReadOnlyObservableCollection<AccountItemViewModel> _accounts;

    [Reactive] private bool _showAddDialog;
    [Reactive] private int _selectedCultureIndex = LocalizationService.Default.CurrentCulture == "de" ? 1 : 0;
    [Reactive] private string _statusMessage = "";

    public string[] Languages => ["English", "Deutsch"];
    public bool ShowAboutButton => !OperatingSystem.IsMacOS();

    public ReadOnlyObservableCollection<AccountItemViewModel> Accounts => _accounts;
    public IEnhancedCommand ShowAddCommand { get; }
    public IEnhancedCommand ShowAboutCommand { get; }
    public IEnhancedCommand ImportFromClipboardCommand { get; }
    public IEnhancedCommand ImportFromFileCommand { get; }
    public IEnhancedCommand ExportToFileCommand { get; }
    public AddAccountViewModel AddAccountViewModel { get; private set; }

    public MainWindowViewModel(
        AccountRepository repository,
        TotpService totpService,
        AddAccountViewModel addAccountViewModel,
        IClipboardService clipboardService,
        IDialogService dialogService,
        IFileDialogService fileDialogService)
    {
        _repository = repository;
        _totpService = totpService;
        _clipboardService = clipboardService;
        _dialogService = dialogService;
        _fileDialogService = fileDialogService;
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

        ImportFromClipboardCommand = ReactiveCommand.CreateFromTask(ImportFromClipboard)
            .Enhance(Loc.ImportFromClipboard, "ImportFromClipboard");

        ImportFromFileCommand = ReactiveCommand.CreateFromTask(ImportFromFile)
            .Enhance(Loc.MenuImportFile, "ImportFromFile");

        ExportToFileCommand = ReactiveCommand.CreateFromTask(ExportToFile)
            .Enhance(Loc.MenuExportFile, "ExportToFile");

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

        var existing = _repository.GetAll();
        var key = GetAccountKey(account);
        if (existing.Any(a => GetAccountKey(a) == key))
        {
            ShowStatusMessage(Loc.DuplicateAccount);
            ctx.SetOutput(Unit.Default);
            return;
        }

        _repository.Insert(account);
        ShowAddDialog = false;
        LoadAccounts();
        ResetAddDialog();
        ctx.SetOutput(Unit.Default);
    }

    private void LoadAccounts()
    {
        var accounts = _repository.GetAll();
        var accountVms = accounts.Select(a => new AccountItemViewModel(a, _totpService, _clipboardService, DeleteAccount, EditAccount, AdvanceCounter));
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
        account.Type = item.EditType;
        account.Issuer = item.EditIssuer.Trim();
        account.Label = item.EditLabel.Trim();
        account.SecretBase32 = OtpAccount.NormalizeSecret(item.EditSecret);
        account.Algorithm = item.EditAlgorithm;
        account.Digits = item.EditDigits;
        account.Period = item.EditPeriod;
        account.HotpCounter = item.EditCounter;

        _repository.Update(account);
        item.RefreshAfterEdit();
        item.IsEditing = false;
    }

    private void AdvanceCounter(AccountItemViewModel item)
    {
        _repository.Update(item.Account);
    }

    private async Task ImportFromClipboard()
    {
        var text = await _clipboardService.GetTextAsync();
        if (text is null)
        {
            ShowImportError();
            return;
        }

        var parsed = OtpAuthUriParser.Parse(text);
        if (parsed is null)
        {
            ShowImportError();
            return;
        }

        AddAccountViewModel.PopulateFrom(parsed);
        ShowAddDialog = true;
    }

    private async Task ImportFromFile()
    {
        var content = await _fileDialogService.OpenAndReadTextFileAsync();
        if (content is null)
            return;

        var lines = content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var existingKeys = _repository.GetAll().Select(GetAccountKey).ToHashSet();

        var imported = 0;
        var duplicates = 0;

        foreach (var line in lines)
        {
            var parsed = OtpAuthUriParser.Parse(line);
            if (parsed is null)
                continue;

            var key = GetAccountKey(parsed);

            if (existingKeys.Contains(key))
            {
                duplicates++;
                continue;
            }

            _repository.Insert(parsed);
            existingKeys.Add(key);
            imported++;
        }

        if (imported > 0)
        {
            LoadAccounts();
            var msg = duplicates > 0
                ? $"{imported} {Loc.Imported}, {duplicates} {Loc.DuplicatesSkipped}"
                : $"{imported} {Loc.Imported}";
            ShowStatusMessage(msg);
        }
        else if (duplicates > 0)
        {
            ShowStatusMessage($"{duplicates} {Loc.DuplicatesSkipped}");
        }
        else
        {
            ShowImportError();
        }
    }

        private void ShowImportError()
    {
        StatusMessage = Loc.ImportParseError;
        Observable.Timer(TimeSpan.FromSeconds(3))
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => StatusMessage = "")
            .DisposeWith(_disposables);
    }

    private async Task ExportToFile()
    {
        var accounts = _repository.GetAll().ToList();
        if (accounts.Count == 0)
        {
            ShowStatusMessage(Loc.NoAccountsToExport);
            return;
        }

        var lines = string.Join(Environment.NewLine, accounts.Select(a => a.ToUri()));
        var success = await _fileDialogService.SaveTextToFileAsync(lines);
        if (success)
            ShowStatusMessage($"{accounts.Count} {Loc.Exported}");
    }

    private void ShowStatusMessage(string message)
    {
        StatusMessage = message;
        Observable.Timer(TimeSpan.FromSeconds(3))
            .ObserveOn(ReactiveUI.RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => StatusMessage = "")
            .DisposeWith(_disposables);
    }

    private static string GetAccountKey(OtpAccount a) =>
        $"{a.Type}|{a.Issuer}|{a.Label}|{OtpAccount.NormalizeSecret(a.SecretBase32)}|{a.Algorithm}|{a.Digits}|{(a.Type == OtpType.Totp ? a.Period.ToString() : a.HotpCounter.ToString())}";

    public void Dispose()
    {
        _disposables.Dispose();
        foreach (var account in _accountsSource.Items)
        {
            account.Dispose();
        }
    }
}
