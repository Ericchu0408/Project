using Android.OS;

namespace SafeDriver;

public partial class AccountSafetySettings : ContentPage
{

    public AccountSafetySettings()
    {
        InitializeComponent();
        SeverPlace.Placeholder = MainPage.serverTempText;
    }
    private async void OnButtonClicked(object sender, EventArgs e)
    {
        MainPage.URL = $"http://{SeverPlace.Text}/DrowsyDrivingService/DDService.ashx?Action=";
        await WriteToFileAsync(SeverPlace.Text);
        MainPage mainPage = new MainPage();
        // 顯示成功訊息
        await DisplayAlert("通知", "設置成功", "確定");

    }


    public async Task WriteToFileAsync(string serveraddress)
    {
        string severFilePath = Path.Combine(FileSystem.AppDataDirectory, "Sever.txt");
        // mainPage = new MainPage();
        MainPage.URL = $"http://{serveraddress}/DrowsyDrivingService/DDService.ashx?Action=";
        await File.WriteAllTextAsync(severFilePath, serveraddress);

    }




}