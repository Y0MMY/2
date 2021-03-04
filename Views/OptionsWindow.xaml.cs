using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace launher
{
    public partial class OptionsWindow : Page
    {
        MainMenu _window;
        private string address = "http://185.231.153.105/";
        const string keyPath = @"HKEY_CURRENT_USER\Software\SAMP"; 

        WebClient web = new WebClient();

        public List<Mod> Mods { get; set; }

        public static string UserFolder { get; set; }

        public OptionsWindow(MainMenu window)
        {
            _window = window;

            //if (File.Exists("./path"))
            //UserFolder = File.ReadAllText("./path");
            UserFolder = Registry.GetValue(keyPath, "gta_sa_exe", "").ToString();



            if (!Directory.Exists($"{UserFolder}/{Constants.ModsFolder}"))
                Directory.CreateDirectory($"{UserFolder}/{Constants.ModsFolder}");

   
            UpdateMods();
            InitializeComponent();
            tbx_currentFolder.Content = UserFolder;
    

        }

        private void chooseDirectoryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new CommonOpenFileDialog();

                dialog.IsFolderPicker = true;

                dialog.ShowDialog();

                tbx_currentFolder.Content = dialog.FileName;
                UserFolder = dialog.FileName;


                //File.WriteAllText("./path", UserFolder);
                Registry.SetValue(keyPath, "gta_sa_exe", UserFolder + "\\gta_sa.exe");

                _window.menuPage.StartDownloads();
            }
            catch
            {
                return;
            }
        }

        

       private void WrapPanel_MouseEnter(object sender, MouseEventArgs e)
        {

            WrapPanel grid = (sender as WrapPanel);
            DoubleAnimation scaleTransform = new DoubleAnimation()
            {
                To = 1.02,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            ScaleTransform scale = new ScaleTransform();
            grid.RenderTransform = scale;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleTransform);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleTransform);


            DoubleAnimation animationShadow = new DoubleAnimation()
            {
                From = 0,
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            DropShadowEffect shadow = new DropShadowEffect()
            {
                Color = Colors.Black,

            };

            grid.Effect = shadow;
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, animationShadow);
        }

        private void WrapPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            WrapPanel grid = (sender as WrapPanel);
            DoubleAnimation scaleTransform = new DoubleAnimation()
            {
                From = 1.02,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            ScaleTransform scale = new ScaleTransform();
            grid.RenderTransform = scale;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleTransform);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleTransform);

            DoubleAnimation animationShadow = new DoubleAnimation()
            {
                From = 0.5,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            DropShadowEffect shadow = new DropShadowEffect()
            {
                Color = Colors.Black
            };

            grid.Effect = shadow;
            shadow.BeginAnimation(DropShadowEffect.OpacityProperty, animationShadow);
        }

        private void UpdateMods()
        {
            string json = web.DownloadString($"{address}getMods.php");

            Mods = JsonConvert.DeserializeObject<List<Mod>>(json);

            foreach(var mod in Mods)        
            {
                string path = $"{UserFolder}/{Constants.ModsFolder}/{mod.ModLink.Substring(mod.ModLink.LastIndexOf("/"))+1}";

                if (File.Exists(path))
                {
                    mod.ButtonText = "Удалить";
                }
                else
                    mod.ButtonText = "Подробнее";

                    
            }
        }

        private void BackToMain(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600)
            };

            DoubleAnimation anim2 = new DoubleAnimation()
            {
                To = 1000,
                Duration = TimeSpan.FromMilliseconds(600)
            };

            _window.optionsFrame.BeginAnimation(Page.WidthProperty, anim);
            _window.mainFrame.BeginAnimation(Page.WidthProperty, anim2);
        }

        private void installMod(object sender, RoutedEventArgs e)
        {
            Mod mod = (sender as Button).DataContext as Mod;
            bool isRoot = mod.ModLink.Substring(mod.ModLink.Length - 3) == "asi";

            if ((sender as Button).Content as string == "Подробнее")
            {            
                ModeWindow dialog = new ModeWindow(mod.Description);
                dialog.ShowDialog();           

                if (dialog.Result == ModeWindow.ModeAnswer.Install)
                {
                 
                    WebClient client = new WebClient();

                    if (isRoot)
                    {
                        client.DownloadFile(mod.ModLink, $"{UserFolder}/{mod.ModLink.Substring(mod.ModLink.LastIndexOf("/")) + 1}");
                    }
                    else
                    {
                        client.DownloadFile(mod.ModLink, $"{UserFolder}/{Constants.ModsFolder}/{mod.ModLink.Substring(mod.ModLink.LastIndexOf("/")) + 1}");
                    }
                    mod.ButtonText = "Удалить";
                }
            }
            else
            {
  
                if (isRoot)
                {
                    File.Delete($"{UserFolder}/{mod.ModLink.Substring(mod.ModLink.LastIndexOf("/")) + 1}");
                }
                else
                {
                    File.Delete($"{UserFolder}/{Constants.ModsFolder}/{mod.ModLink.Substring(mod.ModLink.LastIndexOf("/")) + 1}");
                }

                mod.ButtonText = "Подробнее";
            }
        }

        public long GetFileSize(string url)
        {
            long result = -1;

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }

            return result;
        }

        private void WrapPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = UserFolder;

            dialog.ShowDialog();
        }
    }
}
