using Microsoft.Identity.Client;
using Microsoft.UI.Xaml;
using Shared;
using System.Globalization;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LoginMsalNoneWctDemo
{

    public partial class App : Application
    {
        private Window m_window;

        private static string ClientId = "ba567a8d-18e0-4625-8d38-dd7ae2a41f33";
        private static readonly string AadInstance = "https://login.microsoftonline.com/{0}/v2.0";
        private static readonly string Tenant = "56073e36-70f8-4305-8a33-28d46bdf0ddc";
        private static readonly string Authority = string.Format(CultureInfo.InvariantCulture, AadInstance, Tenant);
        private static IPublicClientApplication _clientApp;

        public static IPublicClientApplication PublicClientApp => _clientApp;

        public App()
        {
            this.InitializeComponent();

            Init();
        }

        private static async void Init()
        {
            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("http://localhost")
                .Build();

            TokenCacheHelper.EnableSerialization(_clientApp.UserTokenCache);

            var accounts = await _clientApp.GetAccountsAsync();

            var firstAccount = accounts.FirstOrDefault();

            if (firstAccount != null)
            {
                var result = await _clientApp.AcquireTokenSilent(MsalClient.Scopes, firstAccount).ExecuteAsync();
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }


    }
}
