using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Timers;
using static SafeDriver.TabPageNotification;
using System.IO;
using System.Threading.Tasks;
using Android.Media;
using System.IO;
using System.Reflection;
using MediaManager;
using System.ComponentModel;

namespace SafeDriver
{
    public partial class MainPage : ContentPage
    {

        public static string DataFile = "1-5.txt";
        public static string serverTempText = "";
        private System.Timers.Timer _timer; // 宣告 Timer 實例
        TabPageNotification tabPageNotification = new TabPageNotification();
        public Dictionary<string, string> alertmsg;
        private DateTime _lastAlertTime; // 保存最後一次發送通知的時間
        static public string URL = "";// $"{SafeDriverServer}/DrowsyDrivingService/DDService.ashx?Action=";
        //$"http://172.20.10.3/DrowsyDrivingService/DDService.ashx?Action=";
        static string lastMinute = "0";
        static bool IsStartFlag = false;
        static bool OneTime = false;
        public MainPage()
        {
            InitializeComponent();
            ReadUrlFromSeverFileAsync();
           
            alertmsg = new Dictionary<string, string>();
            _lastAlertTime = DateTime.MinValue; // 初始化為一個時間

            // 訂閱 AppState 的 RecognitionSwitchChanged 事件，監控開關的變化
            AppState.RecognitionSwitchChanged += OnRecognitionSwitchChanged;

            // 初始化時檢查開關狀態，並啟動計時器（如果需要）
            if (AppState.IsRecognitionSwitchOn)
            {
                StartTimer();
            }
           
        }
        public async Task ReadUrlFromSeverFileAsync()
        {
            string severFilePath = Path.Combine(FileSystem.AppDataDirectory, "Sever.txt");

            if (File.Exists(severFilePath))
            {
                // 如果 Sever.txt 存在，讀取內容
                string serverAddress = await File.ReadAllTextAsync(severFilePath);

                // 設定 MainPage 裡的 URL
                URL = $"http://{serverAddress}/DrowsyDrivingService/DDService.ashx?Action=";

                serverTempText = serverAddress;

                if (OneTime == false)
                    SetDetectData(false);
            }
        }

        // 監聽辨識系統開關狀態變化的方法
        private void OnRecognitionSwitchChanged(bool isOn)
        {
            if (isOn)
            {
                StartTimer(); // 如果開關打開，啟動計時器
            }
            else
            {
                StopTimer(); // 如果開關關閉，停止計時器
            }
        }

        // 初始化並啟動計時器的方法
        private void StartTimer()
        {
            if (IsStartFlag)
                return;
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(15000); // 設定計時器間隔為 5000 毫秒（5 秒）
                _timer.Elapsed += OnTimerElapsed; // 附加 Elapsed 事件的處理程序
                _timer.AutoReset = true; // 每次觸發後自動重設計時器
                _timer.Enabled = true; // 啟用計時器
            }
            IsStartFlag = true;
        }

        // 停止並銷毀計時器的方法
        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Stop();
                _timer.Dispose();
                _timer = null; // 重置計時器為 null
            }
            IsStartFlag = false;
        }

        // 每 5 秒會被呼叫的事件處理程序
        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 在 UI 執行緒上調用 GetDetectData 以確保執行緒安全
            await Dispatcher.DispatchAsync(async () =>
            {


                if (await CheckCondition()) // 如果條件達標
                {

                    // 檢查當前時間與最後發送時間是否在同一分鐘
                    if (DateTime.Now.Minute.ToString() != lastMinute)
                    {
                        lastMinute = DateTime.Now.Minute.ToString();
                        //_lastAlertTime = now; // 更新最後一次發送的時間

                        // 使用 Android MediaPlayer 播放內建資源

                        var afd = Android.App.Application.Context.Resources.OpenRawResourceFd(Resource.Raw.alert_sound); // 請確保 alert_sound.wav 在 Resources/Raw 目錄下

                        var player = new MediaPlayer();
                        player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                        afd.Close();
                        player.Prepare();
                        // 撥放提示音，並在播放完成後停止並釋放播放器
                        player.Start();
                        System.Threading.Thread.Sleep(2000);
                        player.Stop();
                        player.Release();
                        player.Dispose();

                        // 格式化並追加到文字檔
                        string content = FormatAlert(alertmsg);
                        await AppendToTxtFile(content);


                    }

                }
            });
        }

        // 將字典轉換成 Key=Value 格式
        private string FormatAlert(Dictionary<string, string> dict)
        {
            StringBuilder formattedString = new StringBuilder();
            foreach (var kvp in dict)
            {
                formattedString.AppendLine($"{kvp.Key} {kvp.Value}");
            }
            return formattedString.ToString() + "----\n"; // 每次紀錄之間用"----"分隔
        }

        public async Task AppendToTxtFile(string content)
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, DataFile);

            // 如果檔案不存在，則建立一個新檔案
            if (!File.Exists(filePath))
            {
                using (var stream = File.Create(filePath))
                {
                    // 檔案建立後，無需立即寫入內容
                }
            }

            // 讀取檔案的所有行
            var lines = new List<string>(await File.ReadAllLinesAsync(filePath));

            // Append 新的內容到檔案
            var newLines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            lines.AddRange(newLines);

            // 保留最後 120 行
            if (lines.Count > 120)
            {
                lines = lines.Skip(lines.Count - 120).ToList();
            }

            // 將整理後的內容寫回檔案
            await File.WriteAllLinesAsync(filePath, lines);
        }


        private async Task<bool> CheckCondition()
        {
            // 使用 TabPageNotification 的 DetectData 方法
            tabPageNotification.GetDetectData();
            DetectData detectData = tabPageNotification.DetectAllData;

            if (detectData == null)
            {
                // 如果 detectData 是 null，則返回 false 或進行其他處理
                return false;
            }

            // 測試數據
            //detectData.BlinkTimesPerMinutes = 25; // 這裡可以進行測試
            //detectData.YawnTimesPerMinutes = 5; // 這裡可以進行測試

            // 判斷邏輯
            if (detectData.BlinkTimesPerMinutes > 20 || detectData.YawnTimesPerMinutes >= 2)
            {


                if (detectData.BlinkTimesPerMinutes > 20)
                {
                    alertmsg.Clear();
                    alertmsg.Add("描述:", "檢測到駕駛者疲勞，請注意安全！");
                    alertmsg.Add("原因:", "眨眼過多");
                    alertmsg.Add("", $"{DateTime.Now:yyyy-MM-dd HH:mm}");
                    return true;
                }
                else if (detectData.YawnTimesPerMinutes >= 2)
                {
                    alertmsg.Clear();
                    alertmsg.Add("描述:", "檢測到駕駛者疲勞，請注意安全！");
                    alertmsg.Add("原因:", "哈欠過多");
                    alertmsg.Add("", $"{DateTime.Now:yyyy-MM-dd HH:mm}");
                    return true;
                }
            }
            return false; // 沒滿足條件
        }

        public class GarminData
        {
            /// <summary>
            /// 日期
            /// </summary>
            public string Date { get; set; }
            /// <summary>
            /// 睡眠分數
            /// </summary>
            public string OverallSleepScore { get; set; }
            /// <summary>
            /// 睡眠品質
            /// </summary>
            public string SleepQuality { get; set; }
            /// <summary>
            /// 壓力分數
            /// </summary>
            public string AverageStressLevel { get; set; }
            /// <summary>
            /// 壓力等級
            /// </summary>
            public string StressLevel { get; set; }
            /// <summary>
            /// 最小心率
            /// </summary>
            public string MinHeartRate { get; set; }
            /// <summary>
            /// 最大心率
            /// </summary>
            public string MaxHeartRate { get; set; }
            /// <summary>
            /// 平均心率
            /// </summary>
            public string AverageHeartRate { get; set; }
        }

        private async void OnProfilesButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//Profiles");
        }

        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Settings());
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            DateTime selectedDate = datePicker.Date;
            string formattedDate = selectedDate.ToString("yyyy-MM-dd");

            var jsonObj = new
            {
                MethodName = "GetGarminData",
                Date = formattedDate
            };

            string jsonString = JsonConvert.SerializeObject(jsonObj);
            string base64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonString));
            string urlSafeBase64String = Uri.EscapeDataString(base64String);
            // 定義 URL
            string url = URL + $"{urlSafeBase64String}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    try
                    {
                        byte[] data = Convert.FromBase64String(responseBody);
                        string json = System.Text.Encoding.UTF8.GetString(data);
                        GarminData jsonObject = JsonConvert.DeserializeObject<GarminData>(json);
                        // 檢查 OverallSleepScore
                        if (string.IsNullOrEmpty(jsonObject.OverallSleepScore))
                            OverallSleepScore.Text = "";
                        else
                            OverallSleepScore.Text = $"{jsonObject.OverallSleepScore}";
                        // 檢查 SleepQuality
                        if (string.IsNullOrEmpty(jsonObject.SleepQuality))
                            sleepquality.Text = "";
                        else
                            sleepquality.Text = $"{jsonObject.SleepQuality.Substring(jsonObject.SleepQuality.Length - 2)}";
                        // 檢查 AverageHeartRate
                        if (string.IsNullOrEmpty(jsonObject.AverageHeartRate))
                            AverageHeartRate.Text = "";
                        else
                            AverageHeartRate.Text = $"{jsonObject.AverageHeartRate}";
                        // 檢查 StressLevel
                        if (string.IsNullOrEmpty(jsonObject.StressLevel))
                            stresslevel.Text = "";
                        else
                            stresslevel.Text = $"{jsonObject.StressLevel.Substring(jsonObject.StressLevel.Length - 1)}";
                    }
                    catch (FormatException)
                    {
                        var jsonObject = JsonConvert.DeserializeObject(responseBody);
                        Console.WriteLine(jsonObject);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                }
            }
        }

        private async Task SetDetectData(bool isActive)
        {
            try
            {
                if (URL == "")
                    return;

                OneTime = true;
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                };

                // 定義 JSON 物件
                var jsonObj = new
                {
                    MethodName = "SetDetectFlag",
                    DetectFlag = isActive// e.Value.ToString()
                };

                // 將 JSON 物件序列化為字串
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);

                // 將 JSON 字串轉換為 Base64 字串
                string base64String = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonString));

                // 定義 URL
                string url = URL + $"{base64String}";//改IP

                //Console.WriteLine(base64String);

                // 使用 HttpClient 發送 HTTP GET 請求 
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // 確保請求成功
                    if (response.IsSuccessStatusCode)
                    {
                        // 取得回應內容
                        string responseContent = await response.Content.ReadAsStringAsync();

                        AppState.IsRecognitionSwitchOn = isActive;// e.Value; // 設置全域變數
                                                                  // 處理成功
                                                                  //await DisplayAlert("Success", $"Detection flag set to {e.Value.ToString().ToUpper()}.", "OK");
                    }
                    else
                    {
                        // 處理失敗
                        string errorContent = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Error", $"Failed to set detection flag. Server response: {errorContent}", "OK");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                // 處理 HTTP 請求錯誤
                await DisplayAlert("Error", $"HTTP Request error: {httpEx.Message}. Please check your network connection.", "OK");
            }
            catch (Exception ex)
            {
                // 處理其他錯誤
                await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
        }

    }
}
