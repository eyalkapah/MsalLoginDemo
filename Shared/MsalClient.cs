namespace Shared
{
    public class MsalClient
    {
        

        public static string Authority = "https://login.microsoftonline.com/common";

        public static string RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";

        public static string[] Scopes = { "user.read" };
    }
}
