using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Expose
{
    public static class CloudflareService
    {
        private const string CloudflareApiRoot = "https://api.cloudflare.com/client/v4";

        private static void ClearCache()
        {
            listZonesCache = null;
            listDnsRecordsCache = null;
        }

        private static PasteClasses.Cloudflare_ListZones listZonesCache = null;
        public static async Task<PasteClasses.Cloudflare_ListZones> ListZones()
        {
            if (listZonesCache != null)
            {
                return listZonesCache;
            }
            if (!await IsCloudflareAuthReady())
            {
                return null;
            }
            listZonesCache = await Get<PasteClasses.Cloudflare_ListZones>($"{CloudflareApiRoot}/zones");
            return listZonesCache;
        }

        internal const string SetZoneIdCmd = @"[set.cf.config.zone] ""<domain name>""";
        internal static async Task SetZoneId(string[] args)
        {
            if (!await IsCloudflareAuthReady())
            {
                return;
            }
            var zones = await ListZones();
            string zoneName = args[1];
            var zone = zones.result.FirstOrDefault(o => o.name.Equals(zoneName, StringComparison.InvariantCultureIgnoreCase));
            if (zone == null)
            {
                Output.WriteError($"Couldn't find a zone named '{zoneName}'.");
                return;
            }
            await Settings.SetCloudflareZoneId2(zone.id);
        }

        private static PasteClasses.Cloudflare_ListDnsRecords listDnsRecordsCache = null;
        internal static async Task<PasteClasses.Cloudflare_ListDnsRecords> ListDnsRecords(string zoneId)
        {
            if (listDnsRecordsCache != null)
            {
                return listDnsRecordsCache;
            }
            if (!await IsCloudflareAuthReady())
            {
                return null;
            }
            if (string.IsNullOrEmpty(zoneId))
            {
                Output.WriteError($"ZoneId not configured.");
                return null;
            }
            listDnsRecordsCache = await Get<PasteClasses.Cloudflare_ListDnsRecords>($"{CloudflareApiRoot}/zones/{zoneId}/dns_records");
            return listDnsRecordsCache;
        }

        internal static async Task<PasteClasses.CreateOrUpdateDnsRecordResponse> CreateDnsRecord(string zoneId, PasteClasses.CreateOrUpdateDnsRecordRequest request)
        {
            if (!await IsCloudflareAuthReady())
            {
                return null;
            }
            if (string.IsNullOrEmpty(zoneId))
            {
                Output.WriteError($"ZoneId not configured.");
                return null;
            }
            var response = await Post<PasteClasses.CreateOrUpdateDnsRecordResponse>($"{CloudflareApiRoot}/zones/{zoneId}/dns_records", request);
            ClearCache();
            return response;
        }

        internal static async Task<PasteClasses.CreateOrUpdateDnsRecordResponse> UpdateDnsRecord(string zoneId, string recordId, PasteClasses.CreateOrUpdateDnsRecordRequest request)
        {
            if (!await IsCloudflareAuthReady())
            {
                return null;
            }
            if (string.IsNullOrEmpty(zoneId))
            {
                Output.WriteError($"ZoneId not configured.");
                return null;
            }
            var response = await Put<PasteClasses.CreateOrUpdateDnsRecordResponse>($"{CloudflareApiRoot}/zones/{zoneId}/dns_records/{recordId}", request);
            ClearCache();
            return response;
        }

        internal static async Task<PasteClasses.Cloudflare_ListDnsRecords_Result> GetRecord(string zoneId, string recordName, bool suppressOutput = false)
        {
            var records = await ListDnsRecords(zoneId);
            if (records == null)
            {
                return null;
            }
            if (records.result.Length == 0)
            {
                if (!suppressOutput)
                {
                    Output.WriteError($"No DNS records found for zone '{zoneId}'.");
                }
                return null;
            }
            var record = records.result.FirstOrDefault(o => o.name.Equals(recordName, StringComparison.InvariantCultureIgnoreCase));
            if (record == null)
            {
                if (!suppressOutput)
                {
                    Output.WriteError($"No record found for '{recordName}'.");
                }
                return null;
            }
            return record;
        }

        internal static async Task<bool> DoesRecordExist(string zoneId, string recordName)
        {
            var record = await GetRecord(zoneId, recordName, true);
            return record != null;
        }

        internal static async Task<string> GetRecordId(string zoneId, string recordName)
        {
            var record = await GetRecord(zoneId, recordName);
            if (record == null)
            {
                return null;
            }
            return record.id;
        }

        internal static async Task<PasteClasses.CreateOrUpdateDnsRecordResponse> CreateOrUpdateDnsRecord(string zoneId, string recordName, DnsRecordType dnsRecordType, string content, bool proxied)
        {
            var request = new PasteClasses.CreateOrUpdateDnsRecordRequest
            {
                name = recordName,
                content = content,
                proxied = proxied,
                type = dnsRecordType.ToString(),
            };
            var recordExists = await DoesRecordExist(zoneId, recordName);
            if (recordExists)
            {
                var recordId = await GetRecordId(zoneId, recordName);
                return await UpdateDnsRecord(zoneId, recordId, request);
            }
            else
            {
                return await CreateDnsRecord(zoneId, request);
            }
        }

        private static async Task<T> Get<T>(string endPoint)
        {
            var client = new WebClient();
            await AddHeaders(client);
            var responseJson = await client.DownloadStringTaskAsync(endPoint);
            var response = JsonConvert.DeserializeObject<T>(responseJson);
            return response;
        }

        private static Task<T> Post<T>(string endPoint, object request = null)
        {
            return UploadString<T>(endPoint, "POST", request);
        }

        private static Task<T> Put<T>(string endPoint, object request = null)
        {
            return UploadString<T>(endPoint, "PUT", request);
        }

        private static async Task<T> UploadString<T>(string endPoint, string method, object request = null)
        {
            var client = new WebClient();
            await AddHeaders(client);
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            var requestJson = string.Empty;
            if (request != null)
            {
                requestJson = JsonConvert.SerializeObject(request);
            }
            var responseJson = await client.UploadStringTaskAsync(endPoint, method, requestJson);
            var response = JsonConvert.DeserializeObject<T>(responseJson);
            return response;
        }

        private static async Task AddHeaders(WebClient client)
        {
            var authToken = await Settings.GetCloudflareAuthToken();
            if (!string.IsNullOrEmpty(authToken))
            {
                client.Headers.Add("Authorization", $"Bearer {authToken}");
            }
            else
            {
                client.Headers.Add("X-Auth-Email", await Settings.GetCloudflareAuthEmail());
                client.Headers.Add("X-Auth-Key", await Settings.GetCloudflareAuthKey());
            }
        }

        private static async Task<bool> IsCloudflareAuthReady()
        {
            if (!await Settings.IsCloudflareAuthReady())
            {
                App.ShowMenuWithError("Cloudflare configuration missing!");
                return false;
            }
            return true;
        }
    }
}
