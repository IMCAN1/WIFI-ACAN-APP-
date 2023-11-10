using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Windows.Threading;
using NativeWifi;
using System.Windows.Media.Animation;





namespace WIFI_FIX
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // สร้าง DispatcherTimer และตั้งค่าให้เรียก CheckPing ทุก 1 วินาที
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += CheckPing;
        }


        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_App_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Mini_App_Click(object sender, RoutedEventArgs e)
        {
            // ทำการ minimize หน้าต่างหลักของแอปพลิเคชัน
            WindowState = WindowState.Minimized;
        }


        private void Tg_btn_Checked(object sender, RoutedEventArgs e)
        {
            // ใส่โค้ดที่ต้องการเมื่อ Tg_btn ถูกเลือก
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ตรวจสอบสถานะของ Wi-Fi adapter และแสดงผลใน myTextBlock
            string wifiStatus = GetWifiStatus();
            // myTextBlock.Text = $"สถานะ Wi-Fi: {wifiStatus}";
            // เริ่มต้น DispatcherTimer เมื่อหน้าต่างโหลดเสร็จ
            timer.Start();
            // เรียก CheckWifiStatus เมื่อหน้าต่างโหลดเสร็จ
            CheckWifiStatus();
            // CheckWifiStatus2();
            CheckWifiStatus3();
            // CheckWifiStatus4();
            // myTextBlock5.Text = SetAutoConfigEnabled;
            // CheckAndDisplayAutoConfigStatus();
            CheckAndDisplayAutoConfigStatus();

        }

        private void CheckAndDisplayAutoConfigStatus()
        {
            // ตรวจสอบว่า AutoConfig เปิดหรือปิด
            bool isAutoConfigEnabled = IsAutoConfigEnabled();

            // แสดงข้อความใน myTextBlock5
            if (isAutoConfigEnabled)
            {
                myTextBlock5.Text = "AutoConfig enabled";
            }
            else
            {
                myTextBlock5.Text = "AutoConfig disabled";
            }
        }


        private void CheckWifiStatus3()
        {
            string wifiName = GetConnectedWifiName();

            // แสดงผลลัพธ์ใน myTextBlock4
            myTextBlock4.Text = $"กำลังเชื่อมต่อกับ wifi ชื่อ: {wifiName}";
        }

        private string GetConnectedWifiName()
        {
            string wifiName = "ไม่สามารถระบุ Wi-Fi ที่เชื่อมต่อได้";

            try
            {
                WlanClient client = new WlanClient();
                foreach (WlanClient.WlanInterface wlanInterface in client.Interfaces)
                {
                    if (wlanInterface.InterfaceState == Wlan.WlanInterfaceState.Connected)
                    {
                        wifiName = wlanInterface.CurrentConnection.profileName;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // จัดการกับข้อผิดพลาดตามที่คุณต้องการ
                Console.WriteLine($"Error: {ex.Message}");
            }

            return wifiName;
        }



        private void CheckWifiStatus2()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in interfaces)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.OperationalStatus == OperationalStatus.Up)
                {
                    // แสดงข้อความใน myTextBlock
                    // myTextBlock.Text = $"Wireless LAN adapter {nic.Description} is currently active.";

                    // แสดง interfaceDescription ใน myTextBlock4
                    // myTextBlock4.Text = $"Interface name: {nic.Name}";

                    // เรียกใช้งานหรือปิดใช้งาน AutoConfig ของ Wireless LAN adapter ขึ้นอยู่กับปัจจุบัน
                    if (IsAutoConfigEnabled())
                    {
                        // ถ้า AutoConfig ถูกเปิดอยู่ ให้ปิด
                        SetAutoConfigEnabled(false, nic.Name);
                    }
                    else
                    {
                        // ถ้า AutoConfig ถูกปิดอยู่ ให้เปิด
                        SetAutoConfigEnabled(true, nic.Name);
                    }
                }
            }

            // หากไม่พบ Wireless LAN adapter ที่ใช้งาน
            // myTextBlock.Text = "No active Wireless LAN adapter found.";
            // myTextBlock4.Text = "ไม่พบ Interface name ";
        }

        private bool IsAutoConfigEnabled()
        {
            // ตรวจสอบสถานะ AutoConfig โดยใช้ netsh
            Process process = new Process();
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "wlan show settings";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // ตรวจสอบว่า AutoConfig ถูกเปิดหรือปิด
            return output.Contains("Auto configuration logic is enabled on interface");
        }

        private void SetAutoConfigEnabled(bool enable, string interfaces)
        {
            string enableDisable = enable ? "yes" : "no";

            ProcessStartInfo psi = new ProcessStartInfo("netsh")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true, // จะไม่สร้างหน้าต่าง cmd
                Arguments = $"wlan set autoconfig enabled={enableDisable} interface=\"{interfaces}\""
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                process.WaitForExit();

                // แสดงข้อความใน myTextBlock4
                // myTextBlock4.Text = $"Interface name: {interfaces}";

                // ล้างข้อความใน myTextBlock5
                myTextBlock5.Text = "";

                // ถ้า AutoConfig ถูกเปิดอยู่ ให้แสดงข้อความ
                if (enable)
                {
                    myTextBlock5.Text += " (AutoConfig enabled)";
                }
                else
                {
                    myTextBlock5.Text += " (AutoConfig disabled)";
                }
            }
        }



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // เมื่อคลิกที่ปุ่ม StartButton, เรียก CheckWifiStatus2 ใหม่
            CheckWifiStatus2();
        }

        private bool IsWifiConnected()
        {
            try
            {
                // ทดสอบการเชื่อมต่อ Wi-Fi ด้วย ping
                Ping ping = new Ping();
                PingReply reply = ping.Send("1.1.1.1", 400);

                // ถ้าสำเร็จและเชื่อมต่อ Wi-Fi, ส่งค่า true
                return reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // หรือถ้าไม่สำเร็จหรือมีข้อผิดพลาด, ส่งค่า false
                return false;
            }
        }

        private void CheckWifiStatus()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in interfaces)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && nic.OperationalStatus == OperationalStatus.Up)
                {
                    // แสดงข้อความใน myTextBlock3
                    myTextBlock3.Text = $"Wireless LAN adapter {nic.Description} is currently active.";
                    return;  // หยุดทำงานเมื่อพบ Wireless LAN adapter ที่กำลังใช้งาน
                }
            }

            // หากไม่พบ Wireless LAN adapter ที่ใช้งาน
            myTextBlock3.Text = "No active Wireless LAN adapter found.";
        }

        private async void CheckPing(object sender, EventArgs e)
        {
            // ทดสอบ ping ไปยัง 1.1.1.1

            string pingResult = await TestPing("1.1.1.1");

            // เพิ่มผลลัพธ์ลงใน myTextBlock2
            myTextBlock2.Text = $"{DateTime.Now}: {pingResult}\n";
        }


        private async Task<string> TestPing(string ipAddress)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ipAddress);

                if (reply != null && reply.Status == IPStatus.Success)
                {
                    // หากสำเร็จ, หยุดการเล่น Ellipse_Deactivated
                    Ellipse_Deactivated.Storyboard.Stop(this);
                    // เริ่มการเล่น Ellipse_Activated
                    Ellipse_Activated.Storyboard.Begin();
                    // ตั้ง RepeatBehavior ไว้เพื่อให้เล่นตลอดไป
                    Ellipse_Activated.Storyboard.RepeatBehavior = RepeatBehavior.Forever;

                    return $"สำเร็จ (Roundtrip time: {reply.RoundtripTime} ms)";
                }
                else
                {
                    // หน่วงเวลา 1 วินาที
                    await Task.Delay(1000);

                    return "ไม่สำเร็จ";
                }
            }
            catch (Exception ex)
            {
                return $"เกิดข้อผิดพลาด: {ex.Message}";
            }
        }




        private string GetWifiStatus()
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface show interface",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Contains("Wi-Fi"))
                {
                    return "กำลังใช้งาน (Wi-Fi)";
                }
                else
                {
                    return "ไม่ได้เชื่อมต่อ Wi-Fi";
                }
            }
            catch (Exception ex)
            {
                return $"เกิดข้อผิดพลาด: {ex.Message}";
            }
        }

    }
}
