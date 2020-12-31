using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Game.Services.Helpers
{
    public static class Constants
    {
        internal static string audience = "https://gameroomsdev.onmicrosoft.com"; // Get this value from the expose an api, audience uri section example https://appname.tenantname.onmicrosoft.com
        internal static string clientID = "341d6199-54df-46c4-bf4b-df132bb2fb27"; // this is the client id, also known as AppID. This is not the ObjectID
        internal static string tenant = "gameroomsdev.onmicrosoft.com"; // this is your tenant name
        internal static string tenantid = "508fc0b7-54c6-4c27-b9a8-05b018342d63";
        internal static string b2cInstance = "https://gameroomsdev.b2clogin.com";
        internal static string aadInstance = "https://gameroomsdev.b2clogin.com/{0}/v2.0";
        internal static string policyName = "B2C_1_SUSI";
        internal static string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        internal static List<string> validIssuers = new List<string>()
            {
                $"{b2cInstance}/{tenant}/v2.0",
                $"{b2cInstance}/{clientID}/v2.0",
                 $"{b2cInstance}/{tenantid}/v2.0/",
                $"https://login.microsoftonline.com/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0",
                $"https://login.windows.net/{tenant}/",
                $"https://login.microsoft.com/{tenant}/",
                $"https://sts.windows.net/{tenantid}/"
            };
    }
}
