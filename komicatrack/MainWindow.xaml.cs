using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace komicatrack
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        #region 全區域變數及按鈕
        public class global
        {
            public static string browser ;
        }

        //設定瀏覽器按鈕
        private void setbrowser_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
             dialog.ShowDialog();
            if (dialog.FileName !="")
            {
                global.browser = dialog.FileName;
            }
            else
            {
                global.browser = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            }
        }

        //看更新紀錄
        public void story_Click(object sender, RoutedEventArgs e)
        {
            story window = new story();
            window.Show();
        }
        #endregion

        //track 下載html 打開網頁 移動陣列 取字串
        class track
        {
            //下載html
            public string dlhtml(string website)
            {
                try
                {
                WebRequest request = WebRequest.Create(website); 
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine(response.StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string html = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                return html;
                }
                catch { return "0"; }
            }

            //打開網頁
            public void openwebsite(string count)
            {
                try
                {
                    System.Diagnostics.Process.Start(global.browser, count);
                }
                catch { }
            }

            //移動陣列(往上移動)
            public string[] changethread(int thread,string[] content)
            {
                for (int i = thread; i < 5; i++)
                {
                    content[i] = content[i + 1];
                }
                content[5] = "1";
                return content;
            }

            //取出兩字串當中的字
            public string takeout(string html, string word1, string word2)
            {
                int str = html.IndexOf(word1, 0);
                html = html.Substring(str,html.Length-str);
                int end = html.IndexOf(word2, 0);
                html = html.Substring(word1.Length,end-word1.Length);
                return html;
            }

            //把字串最後一次出現前的字拿掉
            public string takeallout(string html ,string word,out int times)
            {
                //找出回應數量
                int rsptimes = 0;
                Regex rsps = new Regex(word);
                foreach (Match match in rsps.Matches(html))
                    rsptimes++;

                //找出最新回應時間
                int latestrsp = 0;
                for (int i = 0; i < rsptimes; i++)
                {
                    latestrsp = html.IndexOf(word, latestrsp);
                    latestrsp = latestrsp + 3;
                }
                times = rsptimes;
                return  html.Remove(0, latestrsp);
            }
        }

        //local 尋找本地版回應跟標題的方法
        class local:track{

            //分辨網站
            private int findweb(string html)
            {
                if ( html.Contains("<html lang=\"zh-TW\">"))
                {
                    return 1;
                }else if (html.Contains("http://www.w3.org/1999/xhtml"))
                {
                    return 2;
                }
                //else if (html.Contains("https://opengraphprotocol.org/schema/"))
                //{
                //    return 3;
                //}
                else
                {
                    return 0;
                }
            }

            //尋找回應
            public string findrsp(string Html)
            {
                //Html 輸入的html
                //html 結算的html
                try
                {
                    int times = 0;
                    string html = "";
                    string time = "";

                    switch (findweb(Html))
                    {
                        case 1:
                            //找出回應數量
                            html = takeallout(Html, "<div class=\"post reply\"", out times);
                            //找出最新回應時間
                            time = takeout(html, "\"now\">", "ID");
                            break;
                        case 2:
                            //找出回應數量
                            html = takeallout(Html, "<div class=\"reply\"", out times);
                            //找出最新回應時間
                            time = takeout(html, "<span class=\"name\"></span> [", "ID");
                            break;
                        //case 3:
                        //    break;
                    }
                    if (findweb(Html) == 0)
                    {
                        return "網址輸入錯誤"; 
                    }
                    else if (times == 0)
                    {
                        return "此討論串無新回應";
                    }
                    else { return "回應數量：" + times + "                               最新回應時間：" + time; }
                }
                catch { return "網址輸入錯誤"; }
            }

            //尋找標題
            public string findtitle(string Html)
            {
                //Html 輸入的html
                try
                {
                    string title = "";
                    switch (findweb(Html))
                    {
                        case 1:
                            //找到發文者
                            title = takeout(Html, "post-head", "\"name\"");
                            //尋找發文者標題
                            title = takeout(Html, "\"title\">", "</span");
                            break;
                        case 2:
                            //找到發文者標題
                            title = takeout(Html, "<span class=\"title\">", "</span>");
                            break;
                        //case 3:
                        //    //找到發文者標題
                        //    title = takeout(Html, "<span class=\"title\">", "</span>");
                        //    break;
                        case 0:
                            title = "網址輸入錯誤";
                            break;
                    }
                   
                    return title;
                }
                catch { return "網址輸入錯誤"; }
            }
        }

        #region 本地版 變數,初始化,按鈕,方法

        //本地板的變數
        public class localglobal
        {
            //宣告討論串當前位置
            public static int threadcount;
            //宣告網址陣列
            public static string[] website =new string [6];  
            //宣告html陣列
            public static string[] webhtml = new string[6]; 
            //宣告標題陣列
            public static string[] title = new string[6];
            //宣告回應陣列
            public static string[] reply = new string[6]; 
        }

        #region 初始化
        private void website_box_GotFocus(object sender, RoutedEventArgs e)
        {
            website_box.Text = "";
        }
        private void komicatrack_Loaded(object sender, RoutedEventArgs e)
        {
            website_box.Text = "在這裡輸入網址";
            load();
        }
        private void komicatrack_Closed(object sender, EventArgs e)
        {
            save();
            Properties.Settings.Default.Save();
        }
        #endregion

        #region 本地按鈕
        //按下新增
        private void str_btm_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.threadcount == 6)
            {
                System.Windows.MessageBox.Show("可追蹤討論串已達上限");
            }
            else
            {
                //將網址放進網址陣列
                localglobal.website[localglobal.threadcount] = website_box.Text;

                //下載html  threadcount為當前討論串位置
                //將網址取出 丟進html取出器
                string html = newlocal.dlhtml(localglobal.website[localglobal.threadcount]);

                //用拿到的網址取出回應跟標題及網頁長度
                localglobal.webhtml[localglobal.threadcount] = html.Length.ToString();
                localglobal.reply[localglobal.threadcount] = newlocal.findrsp(html);
                localglobal.title[localglobal.threadcount] = newlocal.findtitle(html);

                //把拿到的回應跟標題放進去控制項
                switch (localglobal.threadcount)
                {
                    case 0:
                        rspt_label1.Content = localglobal.reply[localglobal.threadcount];
                        title_label1.Content = localglobal.title[localglobal.threadcount];
                        break;
                    case 1:
                        rspt_label2.Content = localglobal.reply[localglobal.threadcount];
                        title_label2.Content = localglobal.title[localglobal.threadcount];
                        break;
                    case 2:
                        rspt_label3.Content = localglobal.reply[localglobal.threadcount];
                        title_label3.Content = localglobal.title[localglobal.threadcount];
                        break;
                    case 3:
                        rspt_label4.Content = localglobal.reply[localglobal.threadcount];
                        title_label4.Content = localglobal.title[localglobal.threadcount];
                        break;
                    case 4:
                        rspt_label5.Content = localglobal.reply[localglobal.threadcount];
                        title_label5.Content = localglobal.title[localglobal.threadcount];
                        break;
                    case 5:
                        rspt_label6.Content = localglobal.reply[localglobal.threadcount];
                        title_label6.Content = localglobal.title[localglobal.threadcount];
                        break;
                }
                //討論串當前位置+1
                localglobal.threadcount = localglobal.threadcount + 1;
            }
        }

        //按下更新
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            check();
        }

        //五個打開網頁按鈕
        #region
        private void gowebsite1_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[0]);
            rsp1_box.Header = "跟串一";
        }

        private void gowebsite2_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[1]);
            rsp2_box.Header = "跟串二";
        }

        private void gowebsite3_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[2]);
            rsp3_box.Header = "跟串三";
        }

        private void gowebsite4_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[3]);
            rsp4_box.Header = "跟串四";
        }

        private void gowebsite5_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[4]);
            rsp5_box.Header = "跟串五";
        }
        private void gowebsite6_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();
            newlocal.openwebsite(localglobal.website[5]);
            rsp5_box.Header = "跟串六";
        }

        #endregion

        //五個刪除按鈕
        #region
        private void del1_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[0] != "")
            {
                ochange(0);
                if (localglobal.threadcount > 0)
                    localglobal.threadcount = localglobal.threadcount - 1;
            }
            else
            {
                title_label1.Content = "標題";
                rspt_label1.Content = "最新回應時間";
            }
        }
        private void del2_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[1] != "")
            {
                ochange(1);
                if (localglobal.threadcount > 0)
                    localglobal.threadcount = localglobal.threadcount - 1;
            }
            else
            {
                title_label2.Content = "標題";
                rspt_label2.Content = "最新回應時間";
            }

        }
        private void del3_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[2] != "")
            {
                ochange(2);
                if (localglobal.threadcount > 0)
                    localglobal.threadcount = localglobal.threadcount - 1;
            }
            else
            {
                title_label3.Content = "標題";
                rspt_label3.Content = "最新回應時間";
            }
        }
        private void del4_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[3] != "")
            {
                ochange(3);
                if (localglobal.threadcount > 0)
                    localglobal.threadcount = localglobal.threadcount - 1;
            }
            else
            {
                title_label4.Content = "標題";
                rspt_label4.Content = "最新回應時間";
            }
        }
        private void del5_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[4] != "")
            {
                ochange(4);
                if (localglobal.threadcount > 0)
                {
                    localglobal.threadcount = localglobal.threadcount - 1;
                } 
            }
            title_label5.Content = "標題";
            rspt_label5.Content = "最新回應時間";
        }
       
        private void del6_Click(object sender, RoutedEventArgs e)
        {
            local newlocal = new local();

            if (localglobal.website[5] != "")
            {
                ochange(5);
                if (localglobal.threadcount > 0)
                {
                    localglobal.threadcount = localglobal.threadcount - 1;
                }
            }
            title_label6.Content = "標題";
            rspt_label6.Content = "最新回應時間";
        }
        #endregion

        #endregion

        #region 本地方法

        //方法：把所有陣列往上移動一層 給刪除按鈕用
        private void ochange(int thread)
        {
            local newlocal = new local();

            //輸入：被指定的陣列位置,陣列全部 輸出：新陣列
            localglobal.title = newlocal.changethread(thread, localglobal.title);
            localglobal.reply = newlocal.changethread(thread, localglobal.reply);
            localglobal.website = newlocal.changethread(thread, localglobal.website);
            localglobal.webhtml = newlocal.changethread(thread, localglobal.webhtml);
            localglobal.title[5] = "標題";
            localglobal.reply[5] = "最新回應時間";
            putcon();
        }

        //確認是否有新回應
        private void check()
        {
            local newlocal = new local();

            ////把現在的網頁長度放進來
            int[] bef = new int[localglobal.website.Length];
            for (int i = 0; i < localglobal.threadcount; i++)
            {
                bef[i] = Int32.Parse(localglobal.webhtml[i]);
            }
            //把所有網頁互相比較
            for (int i = 0; i < localglobal.threadcount; i++)
            {
                string html = newlocal.dlhtml(localglobal.website[i]);

                if (bef[i] < html.Length)
                {
                    localglobal.webhtml[i] = html.Length.ToString();
                    localglobal.title[i] = newlocal.findtitle(html);
                    localglobal.reply[i] = newlocal.findrsp(html);

                    switch (i)
                    {
                        case 0:
                            rsp1_box.Header = "跟串一有新回應";
                            rspt_label1.Content = localglobal.reply[i];
                            break;
                        case 1:
                            rsp2_box.Header = "跟串二有新回應";
                            rspt_label2.Content = localglobal.reply[i];
                            break;
                        case 2:
                            rsp3_box.Header = "跟串三有新回應";
                            rspt_label3.Content = localglobal.reply[i];
                            break;
                        case 3:
                            rsp4_box.Header = "跟串四有新回應";
                            rspt_label4.Content = localglobal.reply[i];
                            break;
                        case 4:
                            rsp5_box.Header = "跟串五有新回應";
                            rspt_label5.Content = localglobal.reply[i];
                            break;
                        case 5:
                            rsp6_box.Header = "跟串六有新回應";
                            rspt_label6.Content = localglobal.reply[i];
                            break;
                    }
                }
            }
        }

        //將陣列文字放進欄位
        private void putcon()
        {
            title_label1.Content = localglobal.title[0];
            title_label2.Content = localglobal.title[1];
            title_label3.Content = localglobal.title[2];
            title_label4.Content = localglobal.title[3];
            title_label5.Content = localglobal.title[4];
            title_label6.Content = localglobal.title[5];

            rspt_label1.Content = localglobal.reply[0];
            rspt_label2.Content = localglobal.reply[1];
            rspt_label3.Content = localglobal.reply[2];
            rspt_label4.Content = localglobal.reply[3];
            rspt_label5.Content = localglobal.reply[4];
            rspt_label6.Content = localglobal.reply[5];
        }
        #endregion

        #endregion

        //讀取檔案
        private void load()
        {
            global.browser = Properties.Settings.Default.browser;
            localglobal.threadcount = Properties.Settings.Default.threadcount;

            for (int i = 0; i < localglobal.website.Length; i++)
            {
                localglobal.website[i] = Properties.Settings.Default.website[i];
            }
            for (int i = 0; i < localglobal.webhtml.Length; i++)
            {
                localglobal.webhtml[i] = Properties.Settings.Default.webhtml[i];
            }
            for (int i = 0; i < localglobal.title.Length; i++)
            {
                localglobal.title[i] = Properties.Settings.Default.title[i];
            }
            for (int i = 0; i < localglobal.reply.Length; i++)
            {
                localglobal.reply[i] = Properties.Settings.Default.reply[i];
            }

            putcon();
            check();
        }
        //存檔
        private void save()
        {
            Properties.Settings.Default.browser = global.browser;
            Properties.Settings.Default.threadcount = localglobal.threadcount;

            for (int i = 0; i < localglobal.website.Length; i++)
            {
                Properties.Settings.Default.website[i] = localglobal.website[i];
            }
            for (int i = 0; i < localglobal.webhtml.Length; i++)
            {
                Properties.Settings.Default.webhtml[i] = localglobal.webhtml[i];
            }
            for (int i = 0; i < localglobal.title.Length; i++)
            {
                Properties.Settings.Default.title[i] = localglobal.title[i];
            }
            for (int i = 0; i < localglobal.reply.Length; i++)
            {
                Properties.Settings.Default.reply[i] = localglobal.reply[i];
            }
        }

        
    }
}
