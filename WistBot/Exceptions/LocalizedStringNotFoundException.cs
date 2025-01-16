namespace WistBot.Exceptions
{
    public class LocalizedStringNotFoundException : Exception
    {
        public LocalizedStringNotFoundException(string key) : base($"Localized string with key {key} not found")
        {
        }
    }
}
