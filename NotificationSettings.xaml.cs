using Microsoft.Maui.Controls;
using System;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Net;

namespace SafeDriver;

public partial class NotificationSettings : ContentPage
{
    public NotificationSettings()
    {
        InitializeComponent();
        WarningNotificationSwitch.IsToggled = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 確保每次進入頁面時通知開關為打開
        WarningNotificationSwitch.IsToggled = true;
    }

    // 通知開關的事件處理
    private async void OnWarningNotificationToggled(object sender, ToggledEventArgs e)
    {
        if (!e.Value) // 如果通知開關被關閉
        {
            await DisplayAlert(
                "警告",
                "關閉警告通知可能導致重要通知遺漏，是否要前往手機設定頁面？",
                "確定"
            );

            // 跳轉到 Android 的應用程式通知設定頁面
            await OpenAppNotificationSettings();
        }
    }

    // 跳轉至 Android 的應用程式詳細設定頁面
    private async Task OpenAppNotificationSettings()
    {
        try
        {
#if ANDROID
            var context = Android.App.Application.Context;
            if (context != null)
            {
                // 使用 Android 的 Uri 類建立包名的 URI
                var uri = Android.Net.Uri.Parse($"package:{context.PackageName}");

                // 跳轉至應用程式的詳細設定頁面
                var intent = new Intent(Settings.ActionApplicationDetailsSettings);
                intent.SetData(uri);
                intent.AddFlags(ActivityFlags.NewTask); // 確保新的 Activity 啟動

                context.StartActivity(intent);
            }
            else
            {
                await DisplayAlert("錯誤", "無法取得應用程式 Context。", "確定");
            }
#else
        await DisplayAlert("通知", "此功能僅適用於 Android 裝置。", "確定");
#endif
        }
        catch (Exception ex)
        {
            // 捕捉並顯示錯誤，防止應用崩潰
            await DisplayAlert("錯誤", $"發生錯誤：{ex.Message}", "確定");
        }
    }
}
