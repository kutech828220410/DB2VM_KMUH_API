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
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
namespace protal_order_batch
{

    class Program
    {
        static private string API_Server = "http://127.0.0.1:4433";
        static private string ServerName = "傳送櫃";
        static private string ServerType = "傳送櫃";


        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // 取得 Console 視窗 Handle
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        // P/Invoke 用来显示和隐藏控制台窗口
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        private static bool consoleAllocated = false;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        const int SW_RESTORE = 9;

        private static NotifyIcon notifyIcon;
        private static Mutex mutex;
        private static ToolStripMenuItem showConsoleMenuItem;
        private static ToolStripMenuItem hideConsoleMenuItem;
        private static bool isConsoleVisible = false;

        [STAThread]
        static void Main(string[] args)
        {
            // 避免重複啟動
            mutex = new Mutex(true, "OnlyRun");
            if (!mutex.WaitOne(0, false))
            {
                return;
            }
            // **初始化系統托盤**
            InitializeTray();

            //// **完全隱藏 Console 窗口**
            HideConsole();
            isConsoleVisible = false;

            // **隱藏程式，不顯示在 Windows 工作列**
            HideFromTaskbar();

         

            // **使用 Timer 週期執行工作**
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000; // 每 5 秒執行一次
            timer.Tick += (sender, e) => ProcessOrders();
            timer.Start();

            Application.Run(); // 確保應用程式保持執行
        }

        static void HideFromTaskbar()
        {
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                // 取得目前視窗的樣式
                int style = GetWindowLong(handle, -20);

                // 設定視窗為工具視窗，這樣它就不會顯示在工作列
                SetWindowLong(handle, -20, style | 0x00000080);
            }
        }

        static void InitializeTray()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Text = "Console App in System Tray",
                Visible = true
            };

            ContextMenuStrip menu = new ContextMenuStrip();
            showConsoleMenuItem = new ToolStripMenuItem("顯示主控台", null, ShowConsole) { Enabled = true };
            hideConsoleMenuItem = new ToolStripMenuItem("隱藏主控台", null, HideConsole) { Enabled = false };
            menu.Items.Add(showConsoleMenuItem);
            menu.Items.Add(hideConsoleMenuItem);
            menu.Items.Add("退出", null, ExitApp);
            notifyIcon.ContextMenuStrip = menu;

            notifyIcon.DoubleClick += ToggleConsole;
        }
        public static void ShowConsole()
        {
            // 檢查是否已經有 Console
            if (!consoleAllocated)
            {
                consoleAllocated = AllocConsole();
                if (!consoleAllocated)
                {
                    AttachConsole(-1);  // 嘗試附加到現有的 Console
                }
            }

            if (consoleAllocated)
            {

                Console.OutputEncoding = Encoding.GetEncoding("Big5");

                // 重新定向标准输出流
                var standardOutput = new StreamWriter(Console.OpenStandardOutput(), Encoding.GetEncoding("Big5"));
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
                Console.SetError(standardOutput);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("* Don't close this console window or the application will also close.");
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                MessageBox.Show("無法顯示控制台窗口");
            }
        }
        public static void HideConsole()
        {
            FreeConsole();
            isConsoleVisible = false;
            // **隱藏程式，不顯示在 Windows 工作列**
            HideFromTaskbar();
            showConsoleMenuItem.Enabled = true;
            hideConsoleMenuItem.Enabled = false;
        }
        static void ShowConsole(object sender, EventArgs e)
        {
            ShowConsole();
            isConsoleVisible = true;
            showConsoleMenuItem.Enabled = false;
            hideConsoleMenuItem.Enabled = true;
        }
        static void HideConsole(object sender, EventArgs e)
        {
            HideConsole();
            isConsoleVisible = false;
            // **隱藏程式，不顯示在 Windows 工作列**
            HideFromTaskbar();
            showConsoleMenuItem.Enabled = true;
            hideConsoleMenuItem.Enabled = false;


            //IntPtr handle = GetConsoleWindow();
            //if (handle != IntPtr.Zero)
            //{
            //    ShowWindow(handle, SW_HIDE);
            //    isConsoleVisible = false;
            //    // **隱藏程式，不顯示在 Windows 工作列**
            //    HideFromTaskbar();
            //    showConsoleMenuItem.Enabled = true;
            //    hideConsoleMenuItem.Enabled = false;
            //}
        }
        static void ToggleConsole(object sender, EventArgs e)
        {
            if (isConsoleVisible)
            {
                HideConsole(sender, e);
            }
            else
            {
                ShowConsole(sender, e);
            }
        }

        static void ExitApp(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Environment.Exit(0);
        }

        static void ProcessOrders()
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
