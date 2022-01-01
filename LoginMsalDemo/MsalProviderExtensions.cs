using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using LoginMsalDemo.Helpers;
using Microsoft.Identity.Client.Extensions.Msal;

namespace LoginMsalDemo
{
    public static class MsalProviderExtensions
    {
        /// <summary>
        /// Helper function to initialize the token cache for non-UWP apps. MSAL handles this automatically on UWP.
        /// </summary>
        /// <param name="provider">The instance of <see cref="MsalProvider"/> to init the cache for.</param>
        /// <param name="storageProperties">Properties for configuring the storage cache.</param>
        /// <param name="logger">Passing null uses the default TraceSource logger.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task InitTokenCacheAsync(
            this MsalCustomProvider provider,
            StorageCreationProperties storageProperties,
            TraceSource logger = null)
        {
#if !WINDOWS_UWP
            // Token cache persistence (not required on UWP as MSAL does it for you)
            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties, logger);
            cacheHelper.RegisterCache(provider.Client.UserTokenCache);
#endif
        }
    }
}
