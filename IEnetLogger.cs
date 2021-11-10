namespace EnetWrappers
{
    public interface IEnetLogger
    {
        void LogDebug(string text, params object[] args);
        void LogInformation(string text, params object[] args);
        void LogWarning(string text, params object[] args);
    }
}
