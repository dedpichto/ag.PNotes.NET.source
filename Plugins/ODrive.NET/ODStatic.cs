namespace ODrive.NET
{
    internal static class ODStatic
    {
        internal const string ClientId = "41e6e99a-e7d5-4799-afd3-53426751026f";
        internal const string ITEM_ID = "ITEM_ID";
        internal const string FOLDER_NAME = "FOLDER_NAME";
        internal const string FILE_NAME = "FILE_NAME";
        internal const string RootUrl = "https://graph.microsoft.com/v1.0/me/drive/root/children";
        internal const string FolderUrl = "https://graph.microsoft.com/v1.0/me/drive/root:/";
        internal const string DeleteUrl = "https://graph.microsoft.com/v1.0/me/drive/items/";
        internal static string ReplaceUrl = $"https://graph.microsoft.com/v1.0/me/drive/items/{ITEM_ID}/content";
        internal static string UploadUrl =
            $"https://graph.microsoft.com/v1.0/me/drive/root:/{FOLDER_NAME}/{FILE_NAME}:/content";
        internal static readonly string[] Scopes = { "user.read", "files.readwrite", "files.readwrite.appfolder" };
    }
}
