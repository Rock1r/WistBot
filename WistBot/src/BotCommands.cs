namespace WistBot.src
{
    public static class BotCommands
    {
        public const string Start = "/start";
        public const string Language = "/language";
        public const string List = "/list";
        public const string Test = "/test";

        public const string AddItem = "/add";
        public const string Help = "help";
        public const string Clear = "clear";
        public const string Remove = "remove";
        public const string Edit = "edit";

        public static IEnumerable<string> AllCommands => new[]
        {
        $"{Start}",
        $"{Language}",
        $"{List}",
        $"{AddItem}",
        $"/{Help}",
        $"/{Clear}",
        $"/{Remove}",
        $"/{Edit}"
    };
    }

}
