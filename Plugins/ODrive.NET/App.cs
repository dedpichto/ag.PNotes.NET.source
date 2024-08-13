using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace ODrive.NET
{
    internal class App
    {
        internal App(string clientId)
        {
            var authority = $"https://login.microsoftonline.com/{Tenant}";

            PublicClientApp = new PublicClientApplication(clientId, authority, TokenCacheHelper.GetUserCache());
        }

        // Below are the clientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - Te content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers
        
        private const string Tenant = "consumers";

        internal PublicClientApplication PublicClientApp { get; }
    }
}
