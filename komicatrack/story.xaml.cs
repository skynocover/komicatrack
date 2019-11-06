using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace komicatrack
{
    /// <summary>
    /// story.xaml 的互動邏輯
    /// </summary>
    public partial class story : Window
    {
        public story()
        {
            InitializeComponent();
        }

        private void history_Loaded(object sender, RoutedEventArgs e)
        {
            //'Added' 添加的新功能
            //'Changed' 功能變更
            //'Deprecated' 不建議使用，未來會刪掉
            //'Removed' 之前不建議使用的功能，這次真的刪掉了
            //'Fixed' 修正的 bug
            //'Security' 修正了安全相關的 bug
            string history = "=== Unreleased ===" + Environment.NewLine +
                            "Added 可以輸入MyKomica板塊" + Environment.NewLine+
                           Environment.NewLine + "=== 1.0.1 2018/3/3 第二版 ===" + Environment.NewLine +
                           "Added 增加可以輸入的串上限一條" + Environment.NewLine +
                            "Added 可以讀取2cat版塊" + Environment.NewLine +
                            "Removed ini檔不再使用 使用Properties.Settings.Default" + Environment.NewLine +
                            "Fixed 設定Grid去控制版面 讓視窗變更大小時不會亂跑" + Environment.NewLine +
                            Environment.NewLine + "=== 1.0.0 2018/3/3 第一版 ===" + Environment.NewLine +
                            "Added 基本功能設定完成 可以使用K島本地板"+ Environment.NewLine +
                            "紀錄跟讀取存串的網站並自動跳出通知 設定瀏覽器的位置等"
                ;
            history_lbe.Content = history;
        }
    }
}
