namespace WistBot
{
    public static class BotCallbacks
    {
        public const string SetName = "set_name";
        public const string SetDescription = "set_description";
        public const string SetLink = "set_link";
        public const string SetMedia = "set_media";
        public const string FinishSetting = "finish_setting";
        public const string English = "en";
        public const string Ukrainian = "uk";

        public static IEnumerable<string> AllCallbacks => new[]
        {
        SetName,
        SetDescription,
        SetLink,
        SetMedia,
        FinishSetting,
        English,
        Ukrainian
    };
    }

}
