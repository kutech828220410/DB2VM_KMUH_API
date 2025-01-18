using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Basic;
using SQLUI;
using System.Text.Json.Serialization;
using HIS_DB_Lib;
using System.Data.Odbc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace protal_order_batch
{
    public class UddsponClass
    {
        [JsonPropertyName("ZUDDSPON")]
        public string 資料異動時戳 { get; set; }
        [JsonPropertyName("HCASETYP")]
        public string 就醫類別 { get; set; }
        [JsonPropertyName("HCASENO")]
        public string 就醫序號 { get; set; }
        [JsonPropertyName("UDORDSEQ")]
        public string 住院序號 { get; set; }
        [JsonPropertyName("HHISNUM")]
        public string 病歷號 { get; set; }
        [JsonPropertyName("HNAMEC")]
        public string 病患姓名 { get; set; }
        [JsonPropertyName("DSPQTY")]
        public string 配藥量 { get; set; }
        [JsonPropertyName("DSPDATE")]
        public string 日期 { get; set; }
        [JsonPropertyName("DSPTIME")]
        public string 時間 { get; set; }
        [JsonPropertyName("HNURSTA")]
        public string 病房 { get; set; }
        [JsonPropertyName("HBED")]
        public string 床號 { get; set; }
        [JsonPropertyName("ORDTYPE")]
        public string 資料種類 { get; set; }
        [JsonPropertyName("UDDRGNO")]
        public string 藥品碼 { get; set; }
        [JsonPropertyName("UDRPNAME")]
        public string 藥品名稱 { get; set; }
        [JsonPropertyName("DSPUNI")]
        public string 包裝單位 { get; set; }
        [JsonPropertyName("HID")]
        public string 醫院別 { get; set; }
        [JsonPropertyName("TAKEPCSN")]
        public string 勤務領藥號 { get; set; }
        [JsonPropertyName("UDFREQN")]
        public string 頻次 { get; set; }
    }
}
