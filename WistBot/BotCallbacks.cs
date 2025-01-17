namespace WistBot
{
    public static class BotCallbacks
    {
        public const string List = "list_callback";
        public const string DeleteList = "delete_list_callback";
        public const string ChangeListName = "change_list_name_callback";
        public const string ChangeVisіbility = "change_visability_callback";

        public const string SetName = "set_name";
        public const string SetDescription = "set_description";
        public const string SetLink = "set_link";
        public const string SetMedia = "set_media";
        public const string FinishSetting = "finish_setting";
        public const string English = "en";
        public const string Ukrainian = "uk";
        public const string ClearList = "clear_list";
        public const string ShareList = "share_list";
        public const string DeleteItem = "delete_item";
        public const string UserList = "user_list";

        public static IEnumerable<string> AllCallbacks => new[]
        {
        SetName,
        SetDescription,
        SetLink,
        SetMedia,
        FinishSetting,
        English,
        Ukrainian,
        List,
        ClearList,
        ShareList
    };
    }

}
