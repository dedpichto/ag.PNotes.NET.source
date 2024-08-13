using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace ODrive.NET
{
    internal static class TokenCacheHelper
    {

        /// <summary>
        /// Get the user token cache
        /// </summary>
        /// <returns></returns>
        public static TokenCache GetUserCache()
        {
            if (_userTokenCache != null) return _userTokenCache;
            _userTokenCache = new TokenCache();
            _userTokenCache.SetBeforeAccess(BeforeAccessNotification);
            _userTokenCache.SetAfterAccess(AfterAccessNotification);
            return _userTokenCache;
        }

        private static TokenCache _userTokenCache;

        /// <summary>
        /// Path to the token cache
        /// </summary>
        public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin";

        private static readonly object FileLock = new object();

        public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.Deserialize(File.Exists(CacheFilePath)
                    ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                        null,
                        DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        public static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (!args.HasStateChanged) return;
            lock (FileLock)
            {
                // reflect changes in the persistent store
                File.WriteAllBytes(CacheFilePath,
                    ProtectedData.Protect(args.TokenCache.Serialize(),
                        null,
                        DataProtectionScope.CurrentUser)
                );
            }
        }
    }
}
