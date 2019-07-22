using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expose
{
    public static class Settings
    {
        private const string CloudflareAuthToken = "{E0CF8FA0-C5B3-4B45-8EDE-29CACEE623DE}";
        private const string CloudflareAuthEmail = "{D2AD2BF6-1679-4EC7-A051-AFCA1DA11426}";
        private const string CloudflareAuthKey = "{0D86E3A2-6E23-4B12-A4CD-231311445456}";
        private const string CloudflareZoneId = "{78368FCA-735F-49F5-9F82-702527FF5DC0}";
        private const string NGrokDnsWildCardRecord = "{5C8E0FDF-0B81-4679-8C3A-4FFF2EDD0D87}";
        private const string NGrokSubDomainInclude = "{0CC688BF-DABB-4B00-A252-C9EC696A35E4}";
        private const string NGrokRegionCode = "{FAEC8EDB-EB2F-45CC-8D1F-4FB3F032D0C7}";

        internal static string[] Cleanse(string[] args) => args?.Select(o => o.Trim('"')).ToArray() ?? new string[0];

        internal static async Task<bool> IsCloudflareAuthReady()
        {
            var authToken = await GetCloudflareAuthToken();
            if (!string.IsNullOrWhiteSpace(authToken))
            {
                return true;
            }
            var key = await GetCloudflareAuthKey();
            var email = await GetCloudflareAuthEmail();
            return !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(email);
        }

        internal const string SetCloudflareAuthTokenCmd = @"[set.cf.auth.token] ""<auth token value>""";
        internal static Task SetCloudflareAuthToken(string[] args)
        {
            return SetSetting("Cloudflare auth token", CloudflareAuthToken, args[1]);
        }

        internal static Task<string> GetCloudflareAuthToken()
        {
            return GetSetting(CloudflareAuthToken);
        }

        internal const string SetCloudflareAuthEmailCmd = @"[set.cf.auth.email] ""<email value>""";
        internal static Task SetCloudflareAuthEmail(string[] args)
        {
            return SetSetting("Cloudflare auth email", CloudflareAuthEmail, args[1]);
        }

        internal static Task<string> GetCloudflareAuthEmail()
        {
            return GetSetting(CloudflareAuthEmail);
        }

        internal const string SetCloudflareAuthKeyCmd = @"[set.cf.auth.key] ""<key value>""";
        internal static Task SetCloudflareAuthKey(string[] args)
        {
            return SetSetting("Cloudflare auth key", CloudflareAuthKey, args[1]);
        }

        internal static Task<string> GetCloudflareAuthKey()
        {
            return GetSetting(CloudflareAuthKey);
        }

        internal const string SetCloudflareZoneIdCmd = @"[set.cf.config.zoneId] ""<zone id>""";
        internal static Task SetCloudflareZoneId(string[] args)
        {
            return SetCloudflareZoneId2(args[1]);
        }

        internal static Task SetCloudflareZoneId2(string zoneId)
        {
            return SetSetting("Cloudflare zone id", CloudflareZoneId, zoneId);
        }

        internal static Task<string> GetCloudflareZoneId()
        {
            return GetSetting(CloudflareZoneId);
        }

        internal const string SetNGrokDnsWildCardRecordCmd = @"[set.ngrok.dns.wildcard] ""<wildcard dns value>""";
        internal static Task SetNGrokDnsWildCardRecord(string[] args)
        {
            var value = args[1];
            if (!value.StartsWith("*."))
            {
                Output.WriteError($"Wildcard records should start with *. like '*.ngrok.beeming.dev'. Value provided is '{value}'.");
            }
            value = value.Remove(0, 2);
            return SetSetting("NGrok wildcard dns", NGrokDnsWildCardRecord, value);
        }

        internal static Task<string> GetNGrokDnsWildCardRecord()
        {
            return GetSetting(NGrokDnsWildCardRecord);
        }

        internal const string SetNGrokSubDomainIncludeCmd = @"[set.ngrok.dns.include] ""<sub domain include value>""";
        internal static Task SetNGrokSubDomainInclude(string[] args)
        {
            var value = args[1];
            value = CleanseSubDomain(value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = $"-{value}";
            }
            return SetSetting("NGrok sub domain include", NGrokSubDomainInclude, value);
        }

        internal static Task<string> GetNGrokSubDomainInclude()
        {
            return GetSetting(NGrokSubDomainInclude);
        }

        internal const string SetNGrokRegionCodeCmd = @"[set.ngrok.region.code] ""<region value>""";
        internal static Task SetNGrokRegionCode(string[] args)
        {
            var value = args[1];
            if (value.Length != 2)
            {
                Output.WriteError($"Region codes are 2 characters normally. Value provided is '{value}'.");
            }
            value = value.ToLowerInvariant();
            return SetSetting("NGrok region code", NGrokRegionCode, value);
        }

        internal static Task<string> GetNGrokRegionCode()
        {
            return GetSetting(NGrokRegionCode);
        }

        private static Task<string> GetSetting(string key)
        {
            try
            {
                var filename = GetSettingsPathFor(key);
                var valueEnc = File.ReadAllText(filename, Encoding.UTF8);
                var value = Encryptor.Decrypt(valueEnc, GetPassword());
                return Task.FromResult(value);
            }
            catch
            {
                return Task.FromResult((string)null);
            }
        }

        private static Task SetSetting(string consoleName, string key, string value)
        {
            var filename = GetSettingsPathFor(key);
            var valueEnc = Encryptor.Encrypt(value, GetPassword());
            File.WriteAllText(filename, valueEnc, Encoding.UTF8);
            Output.WriteLine($"<{consoleName}> value set!");
            return Task.CompletedTask;
        }

        private static string GetPassword() => $"{Environment.UserDomainName}-{Environment.MachineName}";

        private static string GetSettingsPathFor(string key)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Port-Expose", "Settings", $"{key}.data");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filename = Path.Combine(path, $"{key}.data");
            return filename;
        }

        internal static string CleanseSubDomain(string input) => new string(input.Where(o => char.IsLetter(o) || char.IsNumber(o) || o == '-' || o == '_').Select(o => o).ToArray());
    }
}
