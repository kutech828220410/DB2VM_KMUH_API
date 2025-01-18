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
using System.Windows.Forms;
using MyUI;
namespace protal_order_batch
{
    
    class Program
    {
        static private string API_Server = "http://127.0.0.1:4433";
        static private string ServerName = "傳送櫃";
        static private string ServerType = "傳送櫃";



        [STAThread]
        static void Main(string[] args)
        {
            // 初始化 ConsoleTrayUI，設定 Notify 名稱、圖示、循環時間、是否檢查重複執行
            ConsoleTrayUI trayUI = new ConsoleTrayUI(
                notifyTitle: "protal_order_batch",
                icon: SystemIcons.Information, // 可替換為自訂 ICON
                interval: 3000, // 設定循環執行時間為 3 秒
                checkDuplicateExecution: true // 防止重複執行
            );

            // 訂閱顯示事件
            trayUI.OnShowConsole += () =>
            {
                Console.WriteLine("主控台已顯示");
            };

            // 訂閱隱藏事件
            trayUI.OnHideConsole += () =>
            {
                Console.WriteLine("主控台已隱藏");
            };

            // 訂閱循環執行事件
            trayUI.OnCycleExecute += () =>
            {
                try
                {
                    HIS_DB_Lib.OrderClass.init(API_Server, ServerName, ServerType);
                    Uddspon();
                    Udedspon();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                }
            };

            // 運行應用程式（保持系統托盤運行）
            Application.Run();

         
        }

      

        static public void Uddspon()
        {
            MyTimerBasic myTimer = new MyTimerBasic();
            myTimer.TickStop();
            myTimer.StartTickTime(50000);
            Basic.Net.API_Key aPI_Key = new Basic.Net.API_Key("KeyID", "1a4eab02-066f-4b3e-8954-fe641c470a32");
            string json = Basic.Net.WEBApiPostJson("https://oapim.vghks.gov.tw:8065/UDTKService/ud/udtk/ReadUddspon", "{\"hid\":\"3A0\"}", aPI_Key);
            List<UddsponClass> uddsponClasses = json.JsonDeserializet<List<UddsponClass>>();
     
            List<OrderClass> orderClasses = new List<OrderClass>();
            for (int i = 0; i < uddsponClasses.Count; i++)
            {
                OrderClass orderClass = new OrderClass();
                orderClass.GUID = $"{uddsponClasses[i].資料異動時戳};{uddsponClasses[i].病歷號};{uddsponClasses[i].資料種類};{uddsponClasses[i].藥品碼};{uddsponClasses[i].頻次}";
                orderClass.PRI_KEY = $"{uddsponClasses[i].病歷號};{uddsponClasses[i].就醫類別};{uddsponClasses[i].就醫序號};{uddsponClasses[i].住院序號};{uddsponClasses[i].勤務領藥號};{uddsponClasses[i].資料異動時戳}";
                orderClass.藥局代碼 = "UD";
                orderClass.藥品碼 = uddsponClasses[i].藥品碼.Trim();
                orderClass.藥品名稱 = uddsponClasses[i].藥品名稱.Trim();
                orderClass.劑量單位 = uddsponClasses[i].包裝單位.Trim();
                orderClass.交易量 = (uddsponClasses[i].配藥量.Trim().StringToInt32() * -1).ToString();
                orderClass.病人姓名 = uddsponClasses[i].病患姓名.Trim();
                orderClass.開方日期 = $"{uddsponClasses[i].日期} {uddsponClasses[i].時間}";
                orderClass.領藥號 = uddsponClasses[i].勤務領藥號.Trim();
                orderClass.病房 = uddsponClasses[i].病房.Trim();
                orderClass.床號 = uddsponClasses[i].床號.Trim();
                orderClass.頻次 = uddsponClasses[i].頻次.Trim();
                orderClass.病歷號 = uddsponClasses[i].病歷號.Trim();
                orderClass.藥袋類型 = uddsponClasses[i].資料種類.Trim();
                orderClass.產出時間 = DateTime.Now.ToDateTimeString_6();
                orderClass.狀態 = "未調劑";
                //if (orderClass.床號 == "999" && (orderClass.病房.ToUpper().Contains("PER") == false))
                //{
                //    orderClass.病房 = "999";
                //}
                orderClasses.Add(orderClass);
            }
    
            Logger.LogAddLine("Uddspon");
            Logger.Log("Uddspon", $"Uddspon API 讀取共<{uddsponClasses.Count}>筆資料 ,耗時 {myTimer.ToString()} {DateTime.Now.ToDateTimeString()}");
            (int code, string result, List<OrderClass> orderClasses_out) = OrderClass.add_and_updete_by_guid(API_Server, ServerName, ServerType, orderClasses);
            Logger.Log("Uddspon", $"Uddspon result : {result}");
            if (code != 200)
            {
                Logger.LogAddLine("Uddspon");
                return;
            }
            myTimer.TickStop();
            myTimer.StartTickTime(50000);
            string json_out = Basic.Net.WEBApiPostJson("https://oapim.vghks.gov.tw:8065/UDTKService/ud/udtk/WriteUddspon", json, aPI_Key);
            Logger.Log("Uddspon", $"WriteUddspon API result : {json_out}");
            Logger.Log("Uddspon", $"Uddspon API 回寫共<{uddsponClasses.Count}>筆資料 ,耗時 {myTimer.ToString()} {DateTime.Now.ToDateTimeString()}");
            Logger.LogAddLine("Uddspon");
        }
        static public void Udedspon()
        {
            MyTimerBasic myTimer = new MyTimerBasic();
            myTimer.TickStop();
            myTimer.StartTickTime(50000);
            Basic.Net.API_Key aPI_Key = new Basic.Net.API_Key("KeyID", "1a4eab02-066f-4b3e-8954-fe641c470a32");
            string json = Basic.Net.WEBApiPostJson("https://oapim.vghks.gov.tw:8065/UDTKService/ud/udtk/ReadUdedspon", "{\"hid\":\"3A0\"}", aPI_Key);
            List<UdedsponClass> udedsponClasses = json.JsonDeserializet<List<UdedsponClass>>();
    
            List<OrderClass> orderClasses = new List<OrderClass>();
            for (int i = 0; i < udedsponClasses.Count; i++)
            {
                OrderClass orderClass = new OrderClass();
                orderClass.GUID = $"{udedsponClasses[i].資料異動時戳};{udedsponClasses[i].病歷號};{udedsponClasses[i].資料種類};{udedsponClasses[i].藥品碼};{udedsponClasses[i].頻次}";
                orderClass.PRI_KEY = $"{udedsponClasses[i].病歷號};{udedsponClasses[i].就醫類別};{udedsponClasses[i].就醫序號};{udedsponClasses[i].住院序號};{udedsponClasses[i].勤務領藥號};{udedsponClasses[i].資料異動時戳}";
                orderClass.藥局代碼 = "PHER";
                orderClass.藥品碼 = udedsponClasses[i].藥品碼.Trim();
                orderClass.藥品名稱 = udedsponClasses[i].藥品名稱.Trim();
                orderClass.劑量單位 = udedsponClasses[i].包裝單位.Trim();
                orderClass.交易量 = (udedsponClasses[i].配藥量.Trim().StringToInt32() * -1).ToString();
                orderClass.病人姓名 = udedsponClasses[i].病患姓名.Trim();
                orderClass.開方日期 = $"{udedsponClasses[i].日期} {udedsponClasses[i].時間}";
                orderClass.領藥號 = udedsponClasses[i].勤務領藥號.Trim();
                orderClass.病房 = udedsponClasses[i].病房.Trim();
                orderClass.床號 = udedsponClasses[i].床號.Trim();
                orderClass.頻次 = udedsponClasses[i].頻次.Trim();
                orderClass.病歷號 = udedsponClasses[i].病歷號.Trim();
                orderClass.藥袋類型 = udedsponClasses[i].資料種類.Trim();
                orderClass.產出時間 = DateTime.Now.ToDateTimeString_6();
                orderClass.狀態 = "未調劑";
                //if (orderClass.床號 == "999" && (orderClass.病房.ToUpper().Contains("PER") == false))
                //{
                //    orderClass.病房 = "999";
                //}
                orderClasses.Add(orderClass);
            }

            Logger.LogAddLine("Udedspon");
            Logger.Log("Udedspon", $"Udedspon API 讀取共<{udedsponClasses.Count}>筆資料 ,耗時 {myTimer.ToString()} {DateTime.Now.ToDateTimeString()}");
            (int code, string result, List<OrderClass> orderClasses_out) = OrderClass.add_and_updete_by_guid(API_Server, ServerName, ServerType, orderClasses);
            Logger.Log("Udedspon", $"Udedspon result : {result}");
            if (code != 200)
            {
                Logger.LogAddLine("Udedspon");
                return;
            }
            myTimer.TickStop();
            myTimer.StartTickTime(50000);
            string json_out = Basic.Net.WEBApiPostJson("https://oapim.vghks.gov.tw:8065/UDTKService/ud/udtk/WriteUdedspon", json, aPI_Key);
            Logger.Log("Udedspon", $"WriteUddspon API result : {json_out}");
            Logger.Log("Udedspon", $"Udedspon API 回寫共<{udedsponClasses.Count}>筆資料 ,耗時 {myTimer.ToString()} {DateTime.Now.ToDateTimeString()}");
            Logger.LogAddLine("Udedspon");
        }
    }
   
}
