using Microsoft.Maui.Controls;

namespace SafeDriver
{
    public partial class ModifiedAge : ContentPage
    {
        public int SelectedAge { get; set; }

        public ModifiedAge()
        {
            InitializeComponent();

            // 動態添加年齡選項到 Picker
            for (int i = 0; i <= 100; i++)
            {
                AgePicker.Items.Add(i.ToString());
            }

            // 設置事件處理器來處理選擇變更
            AgePicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
        }

        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            // 當選擇變更時更新選擇的年齡
            if (AgePicker.SelectedIndex != -1)
            {
                SelectedAge = int.Parse(AgePicker.SelectedItem.ToString());
                SelectedAgeLabel.Text = $"選擇的年齡：{SelectedAge}";
            }
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            // 儲存選擇的年齡
            // 這裡你可以添加將選擇的年齡保存到資料源的代碼
            // 比如保存到本地存儲或數據庫

            // 假設我們將年齡顯示為已保存
            DisplayAlert("已保存", $"年齡 {SelectedAge} 已保存", "確定");
        }
    }
}
