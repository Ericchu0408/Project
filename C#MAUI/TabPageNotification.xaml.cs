using Newtonsoft.Json;
using System.Text;
using static SafeDriver.MainPage;
using System.Timers;// 引入 Timer 命名空間以便進行定時執行

namespace SafeDriver;

public partial class TabPageNotification : ContentPage
{
   
    public TabPageNotification()
    {
        InitializeComponent();
        //ReadTxtFile();
    }
    public DetectData DetectAllData = new DetectData();

    public class DetectData
    {
        /// <summary>
        /// 眼睛眨眼次數
        /// </summary>
        public int TotalBlinkTimes { get; set; }
        /// <summary>
        /// 眼睛每分鐘眨眼次數
        /// </summary>
        public int BlinkTimesPerMinutes { get; set; }
        /// <summary>
        /// 嘴巴開合次數
        /// </summary>
        public int TotalYawnTimes { get; set; }
        /// <summary>
        /// 嘴巴每分鐘打哈欠次數
        /// </summary>
        public int YawnTimesPerMinutes { get; set; }


    }
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        // ¾É¯è¨ì Settings
        //await _navigation.PushAsync(new Settings());
        Navigation.PushAsync(new Settings());
        //Shell.Current.CurrentPage=
    }
    public async void GetDetectData()
    {


        // 定義 JSON 物件
        var jsonObj = new
        {
            MethodName = "GetDetectData",
        };

        // 將 JSON 物件序列化為字串
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);

        // 將 JSON 字串轉換為 Base64 字串
        string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

        // 對 Base64 字串進行 URL 編碼
        string urlSafeBase64String = Uri.EscapeDataString(base64String);

        // 定義 URL
        string url =MainPage.URL + $"{urlSafeBase64String}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // 發送 GET 請求
                HttpResponseMessage response = await client.GetAsync(url);

                // 確保請求成功
                response.EnsureSuccessStatusCode();

                // 讀取回應內容作為字串
                string responseBody = await response.Content.ReadAsStringAsync();

                // 判斷回應是否為 Base64 格式，嘗試解碼
                try
                {
                    byte[] data = Convert.FromBase64String(responseBody);
                    string json = Encoding.UTF8.GetString(data);
                    DetectAllData = JsonConvert.DeserializeObject<DetectData>(json);
                    // 在這裡處理 jsonObject（例如，更新 UI）
                    if(DetectAllData != null)
                    {
                       // alert.Text = $"TotalYawnTimes :{DetectAllData.TotalYawnTimes} BlinkTimesPerMinutes :{DetectAllData.BlinkTimesPerMinutes} TotalBlinkTimes :{DetectAllData.TotalBlinkTimes} YawnTimesPerMinutes:{DetectAllData.YawnTimesPerMinutes}";//能獲取detect數據
                    }
                        
                }
                catch (FormatException)
                {
                    // 如果回應不是 Base64 格式，直接處理 JSON
                    var jsonObject = JsonConvert.DeserializeObject(responseBody);
                    Console.WriteLine(jsonObject);
                }
            }
            catch (HttpRequestException ex)
            {
                // 處理可能的異常
                Console.WriteLine($"Request error: {ex.Message}");
            }
        }
    }
    public async Task<string> ReadTxtFile()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, MainPage.DataFile);

            if (File.Exists(filePath))
            {
                // 讀取文字檔案的內容
                string fileContent = await File.ReadAllTextAsync(filePath);
                return fileContent; // 回傳讀取的內容
            }
            else
            {
                using (var stream = File.Create(filePath))
                {
                    // 檔案建立後，無需立即寫入內容
                }
                return "檔案";
            }
        }
        catch (Exception ex)
        {
            // 處理錯誤
            return $"讀取檔案時發生錯誤: {ex.Message}";
        }
    }
    // 覆寫 OnAppearing 方法，在頁面顯示時執行
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 讀取 1.TXT 檔案內容
        string content = await ReadTxtFile();

        if (!string.IsNullOrEmpty(content))
        {
            // 在 UI 上顯示讀取的內容
            //await DisplayAlert("檔案內容", content, "OK");
        }

        // 將檔案內容轉換成每一行
        var lines = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 反轉行的順序
        Array.Reverse(lines);

        // 將內容顯示在 ListView 中
        fileContentListView.ItemsSource = lines;
    }
    //private async void ReadFile()
    //{
    //    string content = await ReadTxtFile(); // 預設讀取 123.TXT
    //    if (!string.IsNullOrEmpty(content))
    //    {
    //        // 在 UI 上顯示讀取的內容
    //        await DisplayAlert("檔案內容", content, "OK");
    //    }
    //}
}
