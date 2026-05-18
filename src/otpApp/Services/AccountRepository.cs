using System.Collections.Generic;
using LiteDB;
using otpApp.Models;

namespace otpApp.Services;

public class AccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<OtpAccount> GetAll()
    {
        using var db = new LiteDatabase(_connectionString);
        var col = db.GetCollection<OtpAccount>("accounts");
        return col.Query().OrderByDescending(a => a.CreatedAt).ToList();
    }

    public void Insert(OtpAccount account)
    {
        using var db = new LiteDatabase(_connectionString);
        var col = db.GetCollection<OtpAccount>("accounts");
        col.Insert(account);
    }

    public void Delete(int id)
    {
        using var db = new LiteDatabase(_connectionString);
        var col = db.GetCollection<OtpAccount>("accounts");
        col.Delete(id);
    }
}
