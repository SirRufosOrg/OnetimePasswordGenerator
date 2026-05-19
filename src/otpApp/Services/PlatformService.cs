namespace otpApp.Services;

public class PlatformService : IPlatformService
{
    public bool ShowAboutButton => !OperatingSystem.IsMacOS();
}