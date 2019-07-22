using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Expose
{
    internal class App
    {
        internal static Func<string[], Task> GetCommand(string[] args)
        {
            switch (args.Length)
            {
                case 2:
                    return CheckTwoArgCommands(args);
                case 4:
                    return CheckFourArgCommands(args);
                default:
                    return null;
            }
        }

        private static Func<string[], Task> CheckTwoArgCommands(string[] args)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "set.cf.auth.token":
                    return Settings.SetCloudflareAuthToken;
                case "set.cf.auth.email":
                    return Settings.SetCloudflareAuthEmail;
                case "set.cf.auth.key":
                    return Settings.SetCloudflareAuthKey;
                case "set.cf.config.zoneId":
                    return Settings.SetCloudflareZoneId;
                case "set.cf.config.zone":
                    return CloudflareService.SetZoneId;
                case "set.ngrok.dns.wildcard":
                    return Settings.SetNGrokDnsWildCardRecord;
                case "set.ngrok.dns.include":
                    return Settings.SetNGrokSubDomainInclude;
                case "set.ngrok.region.code":
                    return Settings.SetNGrokRegionCode;
            }
            return null;
        }

        private static Func<string[], Task> CheckFourArgCommands(string[] args)
        {
            switch (args[0].ToLowerInvariant())
            {
                case "port":
                    if (args[2].Equals("as", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Commands.ExposePort;
                    }
                    break;
                case "s-port":
                    if (args[2].Equals("as", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Commands.ExposeSecurePort;
                    }
                    break;
            }
            return null;
        }

        internal static void ShowMenu()
        {
            Output.Write($@"
Expose https dev site using ngrok and Cloudflare
================================================

__*Config*__
__*======*__
expose {Settings.SetCloudflareAuthTokenCmd} __*OR*__ expose {Settings.SetCloudflareAuthEmailCmd}
                                               expose {Settings.SetCloudflareAuthKeyCmd}
expose {Settings.SetCloudflareZoneIdCmd} __*OR*__ expose {CloudflareService.SetZoneIdCmd}
expose {Settings.SetNGrokDnsWildCardRecordCmd}
expose {Settings.SetNGrokSubDomainIncludeCmd}
expose {Settings.SetNGrokRegionCodeCmd}

__*Usage*__
__*=====*__
expose {Commands.ExposePortCmd}
expose {Commands.ExposeSecurePortCmd}

");
        }

        internal static void ShowMenuWithError(string errorMessage)
        {
            Output.WriteError(errorMessage);
            ShowMenu();
        }
    }
}