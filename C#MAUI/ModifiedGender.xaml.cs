using Microsoft.Maui.Controls;
using SafeDriver;

namespace SafeDriver
{
    public partial class ModifiedGender : ContentPage
    {
        public string SelectedGender { get; set; }

        public ModifiedGender()
        {
            InitializeComponent();

            // 添加性別選項到 Picker
            GenderPicker.Items.Add("男性");
            GenderPicker.Items.Add("女性");
            GenderPicker.Items.Add("其他");

            // 設置事件處理器來處理選擇變更
            GenderPicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        }

        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            // 當選擇變更時更新選擇的性別
            if (GenderPicker.SelectedIndex != -1)
            {
                SelectedGender = GenderPicker.SelectedItem.ToString();
                SelectedGenderLabel.Text = $"選擇的性別：{SelectedGender}";
            }
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // 儲存選擇的性別
            // 這裡你可以添加將選擇的性別保存到資料源的代碼
            // 比如保存到本地存儲或數據庫

            // 假設我們將性別顯示為已保存
            DisplayAlert("已保存", $"性別 {SelectedGender} 已保存", "確定");
        }
    }
}
