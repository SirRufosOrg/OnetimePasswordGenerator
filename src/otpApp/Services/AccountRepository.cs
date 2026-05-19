namespace otpApp.Services;

public class AccountRepository : IAccountRepository, IDisposable
{
    private readonly LiteDatabase _db;

    public AccountRepository(string connectionString)
    {
        _db = new LiteDatabase(connectionString);
    }

    public List<OtpAccount> GetAll()
    {
        var col = _db.GetCollection<OtpAccount>("accounts");
        return col.Query().OrderByDescending(a => a.CreatedAt).ToList();
    }

    public void Insert(OtpAccount account)
    {
        var col = _db.GetCollection<OtpAccount>("accounts");
        col.Insert(account);
    }

    public void Update(OtpAccount account)
    {
        var col = _db.GetCollection<OtpAccount>("accounts");
        col.Update(account);
    }

    public void Delete(int id)
    {
        var col = _db.GetCollection<OtpAccount>("accounts");
        col.Delete(id);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
