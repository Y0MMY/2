using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace launher
{
    public partial class MainMenu : Window
    {
        private string ServerAddress = "http://185.231.153.105/";
        private Dictionary<string, long> FilesToUpdate = new Dictionary<string, long>();
        private bool isUpdating = false;

        public MainMenuPage menuPage;
        public OptionsWindow optionsPage;
        public int BufferSize = 1000000;
        public long DownloadedSize;
        public long CurrentSize;
        string CurrentFile;
        public List<Game> Games = new List<Game>();
        WebClient web = new WebClient();

        InitWindow initWindow;

        private void CreateInitWindow()
        {
            initWindow = new InitWindow();
            initWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

        public MainMenu()
        {     
            optionsPage = new OptionsWindow(this);

            Thread newWindowThread = new Thread(new ThreadStart(CreateInitWindow));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

            menuPage = new MainMenuPage(this);

            initWindow.Dispatcher.BeginInvoke(
(Action)(() => { initWindow.SetOperation("Проверка лаунчера"); }));

            menuPage.CheckLauncher();

            initWindow.Dispatcher.BeginInvoke(
(Action)(() => { initWindow.SetOperation("Проверка игры"); }));


            initWindow.Dispatcher.BeginInvoke(
(Action)(() => { initWindow.SetOperation("Инициализация"); }));


            InitializeComponent();
            mainFrame.Navigate(menuPage);
            optionsFrame.Navigate(optionsPage);

            initWindow.Dispatcher.BeginInvoke(
(Action)(() => { initWindow.Close(); }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            menuPage.StartDownloads();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void buttonOptionsClick(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                To = 1000,
                Duration = TimeSpan.FromMilliseconds(600)
            };
            DoubleAnimation anim2 = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600)
            };

            optionsFrame.BeginAnimation(Frame.WidthProperty, anim);
            mainFrame.BeginAnimation(Frame.WidthProperty, anim2);
        }

        private void ShowOptions(object sender, EventArgs args)
        {
            DoubleAnimation anim = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            optionsPage.BeginAnimation(Page.OpacityProperty, anim);

            if (!mainFrame.CanGoForward)
                mainFrame.Navigate(optionsPage);
            else
                mainFrame.GoForward();
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/ybnV9W");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Process.Start("https://youtube.com");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Process.Start("https://vk.com/danyarob");
        }
    }
}
