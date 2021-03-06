using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginMsalDemo
{
    class CacheConfig
    {
        public const string CacheFileName = "msal_cache.dat";
        public const string CacheDir = "MSAL_CACHE";

        public const string KeyChainServiceName = "msal_service";
        public const string KeyChainAccountName = "msal_account";

        public const string LinuxKeyRingSchema = "com.contoso.devtools.tokencache";
        public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
        public const string LinuxKeyRingLabel = "MSAL token cache for all Contoso dev tool apps.";
        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "MyApps");
    }
}
