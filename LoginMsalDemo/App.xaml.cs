using CommunityToolkit.Authentication;
using CommunityToolkit.Authentication.Extensions;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.UI.Xaml;
using Shared;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LoginMsalDemo
{

    public partial class App : Application
    {
        // fill your application client id here
        public static string ClientId = "";
        
        public App()
        {
            this.InitializeComponent();

            InitializeGlobalProvider();
        }

        private async void InitializeGlobalProvider()
        {
            if (ProviderManager.Instance.GlobalProvider == null)
            {
                var provider = new MsalProvider(ClientId, MsalClient.Scopes, null, true);

                // Configure the token cache storage for non-UWP applications.
                var storageProperties = new StorageCreationPropertiesBuilder(CacheConfig.CacheFileName, CacheConfig.CacheDir)
                    .WithLinuxKeyring(
                        CacheConfig.LinuxKeyRingSchema,
                        CacheConfig.LinuxKeyRingCollection,
                        CacheConfig.LinuxKeyRingLabel,
                        CacheConfig.LinuxKeyRingAttr1,
                        CacheConfig.LinuxKeyRingAttr2)
                    .WithMacKeyChain(
                        CacheConfig.KeyChainServiceName,
                        CacheConfig.KeyChainAccountName)
                    .Build();
                await provider.InitTokenCacheAsync(storageProperties);

                ProviderManager.Instance.GlobalProvider = provider;

                await provider.TrySilentSignInAsync();
            }
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
