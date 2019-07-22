using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expose
{
    public static class PasteClasses
    {

        public class Cloudflare_ListZones
        {
            public Cloudflare_ListZones_Result[] result { get; set; }
            public Cloudflare_ListZones_Result_Info result_info { get; set; }
            public bool success { get; set; }
        }

        public class Cloudflare_ListZones_Result_Info
        {
            public int page { get; set; }
            public int per_page { get; set; }
            public int total_pages { get; set; }
            public int count { get; set; }
            public int total_count { get; set; }
        }

        public class Cloudflare_ListZones_Result
        {
            public string id { get; set; }
            public string name { get; set; }
            public string status { get; set; }
            public bool paused { get; set; }
            public string type { get; set; }
            public int development_mode { get; set; }
            public string[] name_servers { get; set; }
            public string[] original_name_servers { get; set; }
            public object original_registrar { get; set; }
            public string original_dnshost { get; set; }
            public DateTime modified_on { get; set; }
            public DateTime created_on { get; set; }
            public DateTime activated_on { get; set; }
            public string[] permissions { get; set; }
        }



        public class Cloudflare_ListDnsRecords
        {
            public Cloudflare_ListDnsRecords_Result[] result { get; set; }
            public Cloudflare_ListDnsRecords_Result_Info result_info { get; set; }
            public bool success { get; set; }
            public object[] errors { get; set; }
            public object[] messages { get; set; }
        }

        public class Cloudflare_ListDnsRecords_Result_Info
        {
            public int page { get; set; }
            public int per_page { get; set; }
            public int total_pages { get; set; }
            public int count { get; set; }
            public int total_count { get; set; }
        }

        public class Cloudflare_ListDnsRecords_Result
        {
            public string id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string content { get; set; }
            public bool proxiable { get; set; }
            public bool proxied { get; set; }
            public int ttl { get; set; }
            public bool locked { get; set; }
            public string zone_id { get; set; }
            public string zone_name { get; set; }
            public DateTime modified_on { get; set; }
            public DateTime created_on { get; set; }
            public int priority { get; set; }
        }



        public class CreateOrUpdateDnsRecordRequest
        {
            public string type { get; set; }
            public string name { get; set; }
            public string content { get; set; }
            public bool proxied { get; set; }
        }



        public class CreateOrUpdateDnsRecordResponse
        {
            public CreateOrUpdateDnsRecordResponse_Result result { get; set; }
            public bool success { get; set; }
            public object[] errors { get; set; }
            public object[] messages { get; set; }
        }

        public class CreateOrUpdateDnsRecordResponse_Result
        {
            public string id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string content { get; set; }
            public bool proxiable { get; set; }
            public bool proxied { get; set; }
            public int ttl { get; set; }
            public bool locked { get; set; }
            public string zone_id { get; set; }
            public string zone_name { get; set; }
            public DateTime modified_on { get; set; }
            public DateTime created_on { get; set; }
        }

    }
}
