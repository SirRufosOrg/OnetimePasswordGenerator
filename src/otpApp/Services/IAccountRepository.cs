namespace otpApp.Services;

public interface IAccountRepository : IDisposable
{
    List<OtpAccount> GetAll();
    void Insert(OtpAccount account);
    void Update(OtpAccount account);
    void Delete(int id);
}