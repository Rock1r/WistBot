namespace WistBot.src
{
    public static class Button
    {
        public const string English = "🇬🇧 English";
        public const string Ukrainian = "🇺🇦 Українська";
        public const string SetName = "set_name_button";
        public const string ChangeName = "set_new_name_button";
        public const string SetDescription = "set_description_button";
        public const string SetLink = "set_link_button";
        public const string SetMedia = "set_media_button";
        public const string FinishSetting = "finish_setting_button";
        public const string AddItem = "add_list_item_button";
        public const string AddList = "add_list_button";
        public const string List = "list_button";
        public const string ClearList = "clear_list_button";
        public const string ShareList = "share_list_button";
        public const string DeleteList = "delete_list_button";
        public const string ChangeListName = "change_list_name";

        public static IEnumerable<string> AllButtons => new[]
        {
            $"{English}",
            $"{Ukrainian}",
            $"{SetName}",
            $"{ChangeName}",
            $"{SetDescription}",
            $"{SetLink}",
            $"{SetMedia}",
            $"{FinishSetting}",
            $"{AddItem}",
            $"{List}",
            $"{ClearList}",
            $"{ShareList}"
        };
    }
}
