using Microsoft.Maui.Controls;

namespace SafeDriver
{
    public partial class TabPageDevice : ContentPage
    {
        public TabPageDevice()
        {
            InitializeComponent();
        }

        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            // 導航到 Settings
            //await _navigation.PushAsync(new Settings());
            Navigation.PushAsync(new Settings());
            //Shell.Current.CurrentPage=
        }
        // 新增裝置按鈕的事件處理器
        private async void OnAddDeviceButtonClicked(object sender, EventArgs e)
        {
            // 顯示彈出框提醒使用者開啟藍芽和與 Garmin 連接
            await DisplayAlert("新增裝置",
                "\r\n1.「開啟手機的藍牙」\r\n2.「與Garmin手環進行配對」",
                "確定");
        }
    }
}
