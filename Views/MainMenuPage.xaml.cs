using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using SampQueryApi;

namespace launher
{
    /// <summary>
    /// Interaction logic for MainMenuPage.xaml
    /// </summary>
    public partial class MainMenuPage : Page, INotifyPropertyChanged
    {
        private WebClient web = new WebClient();

    
        private int allOnline;
        private Game currentDowloading;
        private Downloader currentDownloader;
        private Server currentServer;
        public List<News> News { get; set; }
        public List<Server> Servers { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        Downloader launcherUpdater;
        Downloader downloader;
        bool launcherUpdate;
        private Timer updaterTimer;

        public Downloader CurrentDownloader
        {
            get => currentDownloader;
            set
            {
                currentDownloader = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDownloader)));
            }
        }

        public Game CurrentDownloading
        {
            get => currentDowloading;
            set
            {
                currentDowloading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDownloading)));
            }
        }

        public int AllOnline
        {
            get => allOnline;
            set
            {
                allOnline = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllOnline)));
            }
        }

        public Server CurrentServer
        {
            get => currentServer;
            set
            {
                currentServer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentServer)));
            }
        }

        public List<Game> Games { get; set; } = new List<Game>();
        MainMenu window;

        public MainMenuPage(MainMenu window)
        {
            Rectangle rec = new Rectangle();
            Servers = new List<Server>();
            this.window = window;
            Servers.Add(new Server() { Address = "194.61.44.61", Port = 7777, Name = "1" });
            Servers.Add(new Server() { Address = "5.254.123.4", Port = 7777, Name = "2" });
            Servers.Add(new Server() { Address = "194.61.44.64", Port = 7777, Name = "3" });

            int index;
            if (File.Exists("./server"))
            {
                if (Int32.TryParse(File.ReadAllText("./server"), out index) && index < Servers.Count)
                {
                    CurrentServer = Servers[--index];                                 
                }
                else
                {
                    CurrentServer = Servers[0];
                    index = 0;
                }
            }
            else
            {
                CurrentServer = Servers[0];
                index = 0;
            }

            updaterTimer = new Timer(UpdateServers, null, 0, 5000);
           
            UpdateNews();

          
       
           InitializeComponent();
            servers.SelectedIndex = index;
        }

        private void UpdateServers(object state)
        {
            int temp = 0;

            foreach (var s in Servers)
            {
                try
                {
                    SampQuery query = new SampQuery(s.Address, s.Port, 'i');

                    Dictionary<string, string> result = query.read();

                    s.MaxPlayers = Int32.Parse(result["maxplayers"]);
                    s.CurrentPlayers = Int32.Parse(result["players"]);
                    temp += s.CurrentPlayers;
                }
                catch
                {
                    continue;
                }
            }

            AllOnline = temp;
        }

      
        private void UpdateNews()
        {
                string json = web.DownloadString($"{Constants.ServerAddress}/getNews.php");

                News = JsonConvert.DeserializeObject<List<News>>(json);
        }

        public void CheckLauncher()
        {
            launcherUpdater = new Downloader(Constants.ServerAddress, Constants.ServerLauncherFolder, "./");
            launcherUpdate = launcherUpdater.CheckLauncherFiles();
        }       

        public async void StartDownloads()
        {
         //if(launcherUpdate == true)
         //    {
         //        CurrentDownloader = launcherUpdater;
         //        launcherUpdater.PercentageUpdated += UpdateProgressBar;
         //        Task.Run(launcherUpdater.StartUpdateLauncher);
         //         return;
         //    }   

         //   downloader = new Downloader(Constants.ServerAddress, Constants.ServerFolder, OptionsWindow.UserFolder);

         //   if (OptionsWindow.UserFolder == null)
         //   {
         //       FirstPathWindow dialog = new FirstPathWindow();
         //       dialog.ShowDialog();
               
         //   }

         //   downloader.UserFolder = OptionsWindow.UserFolder;
         //   downloader.PercentageUpdated += UpdateProgressBar;
         //   CurrentDownloader = downloader;

           
         //   if (File.Exists($"{OptionsWindow.UserFolder}/data.txt") && File.ReadAllText($"{OptionsWindow.UserFolder}/data.txt") != "Downloading")
         //   {
         //       Task.Run(downloader.StartUpdateGame);
         //   }
         //   else
         //   {
         //       Task.Run(downloader.DownloadGame);
         //   }

        } 

        private void UpdateProgressBar(object sender, EventArgs args)
        {
           
            progressBar.Dispatcher.BeginInvoke(
       (Action)(() => {
           DoubleAnimation animation = new DoubleAnimation()
           {
               To = 100 - (sender as Downloader).Percentage,
               Duration = TimeSpan.FromMilliseconds(200)
           };
           progressBar.BeginAnimation(ProgressBar.ValueProperty, animation); })); 
        }

        private void btn_chooseServer_Click(object sender, RoutedEventArgs e)
        {
            if(arrow_rotate.Angle == 0)
              listOfServers.IsOpen = !listOfServers.IsOpen;

            double angle = 0;

            if (arrow_rotate.Angle == 0)
                angle = 180;

            DoubleAnimation animation = new DoubleAnimation()
            {
                To = angle,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            arrow_rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void servers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentServer = servers.SelectedItem as Server;

            File.WriteAllText("./server", CurrentServer.Name);
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(downloader !=null && downloader.CurrentStatus != Downloader.Status.Ready)
            {
                Message message = new Message("Игра еще не готова", Message.MessageButtons.Ok);
                message.ShowDialog();
            }
            else if(launcherUpdater?.CurrentStatus != Downloader.Status.Ready)
            {
                Message message = new Message("Лаунчер еще не готов", Message.MessageButtons.Ok);
                message.ShowDialog();
            }
            else
            {
                Message message = new Message("Лаунчер еще не готов", Message.MessageButtons.Ok);
                message.ShowDialog();
            }

        }

        private void showNews(object sender, RoutedEventArgs e)
        {

            Process.Start(((sender as Button).DataContext as News).PageLink);
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(listOfServers.IsOpen)
               btn_chooseServer_Click(this, null);
        }

        private void Page_LostFocus(object sender, RoutedEventArgs e)
        {
            if (listOfServers.IsOpen)
                btn_chooseServer_Click(this, null);
        }

        private void listOfServers_Closed(object sender, EventArgs e)
        {

            DoubleAnimation animation = new DoubleAnimation()
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            arrow_rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }
    }
}
