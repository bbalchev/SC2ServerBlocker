namespace SC2ServerBlocker.Firewall
{
    public interface IAdministratorChecker
    {
        bool IsRunningAsAdministrator();
    }
}
