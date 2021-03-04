using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace launher
{
    public class Game: INotifyPropertyChanged
    {
        private WebClient web;
        private Timer timerUpdateProgress;
        private string currentFile;
        private double downloadSpeed;
        private long currentSize;
        private long allSize;
        private int percentage;


        public enum Status
        {
            Downloading,
            Updating,
            Ready
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public static string ServerAddress { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int MaxPlayers { get; set; }
        public int MinPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int BufferSize { get; set; } = 1000000;
        public long DownloadedSize { get; set; }
        public Status CurrentStatus { get; set; }
        public Dictionary<string, long> FilesToUpdate = new Dictionary<string, long>();
        public long CurrentSize
        {
            get => currentSize;
            set
            {
                currentSize = value;
                OnPropertyChanged(nameof(CurrentSize));
            }
        }
        public double DownloadSpeed
        {
            get => downloadSpeed;
            set
            {
                downloadSpeed = value;
                OnPropertyChanged(nameof(DownloadSpeed));
            }
        }
        public string CurrentFile
        {
            get => currentFile;
            set
            {
                currentFile = value;
                OnPropertyChanged(nameof(CurrentFile));
            }
        }

        public long AllSize
        {
            get => allSize;
            set
            {
                allSize = value;
                OnPropertyChanged(nameof(AllSize));
            }
        }

        public int Percentage
        {
            get => percentage;
            set
            {
                percentage = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }

        public Game(WebClient client, string serverAddress, string path, string name)
        {
            web = client;
            ServerAddress = serverAddress;
            Path = path;
            Name = name;

            if (!File.Exists($"{path}/data.txt"))
            {
                CurrentStatus = Status.Downloading;
                File.WriteAllText($"{path}/data.txt", Status.Downloading.ToString());
            }
            else
                CurrentStatus = (Status)Enum.Parse(typeof(Status), File.ReadAllText($"{path}/data.txt"));


            if(File.Exists($"../Files/{Name}/version"))
                CheckVersion();

            AllSize = Int64.Parse(web.DownloadString($"{ServerAddress}folderSize.php?Folder={path}"));
            CurrentSize = CalculateSize(path);
        }

        public void CheckVersion()
        {
            string version = web.DownloadString($"{ServerAddress}fileTimestamp.php?Path=Files/{Name}/version");
            if (File.ReadAllText($"../Files/{Name}/version") != version)
            {
                CurrentStatus = Status.Updating;
                File.WriteAllText($"../Files/data.txt", Status.Updating.ToString());
            }
        }

        public void StartDownloadGame()
        {
            string currentPath = $"../Files/{Name}";

            timerUpdateProgress = new Timer(UpdateProgressBar, null, 1000, 1000);
 
            DownloadFolder(currentPath);
            timerUpdateProgress.Dispose();
            CurrentStatus = Status.Ready;
            File.WriteAllText($"Files/{Name}/data.txt", Status.Ready.ToString());
            UpdateVersion();
        }

        private void CheckFilesToUpdate(string folder)
        {

            string[] files = web.DownloadString($"{ServerAddress}getFiles.php?Folder={folder}/").Split('\n');
            byte[] buffer;
            MD5 hasher = MD5.Create();

            foreach (var f in files)
            {
                if (f == "version" || f == "," || f == "")
                    continue;

                string pathFile = $"{folder}/{f}";

                if(!File.Exists(pathFile))
                {
                    FilesToUpdate[pathFile] = Int64.Parse(web.DownloadString($"{ServerAddress}fileSize.php?Path={pathFile}"));
                    continue;
                }

                byte[] hash = web.DownloadData($"{ServerAddress}hashFile.php?Path={folder}/{f}");

              

                using (var stream = File.OpenRead(pathFile)) {

                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    buffer = hasher.ComputeHash(buffer);
                }

              

                if (!hash.SequenceEqual(buffer))
                {
                    FilesToUpdate[pathFile] = Int64.Parse(web.DownloadString($"{ServerAddress}fileSize.php?Path={pathFile}"));
                    File.Delete(pathFile);
                }
            }


            string[] directories = web.DownloadString($"{ServerAddress}getFolders.php?Folder={folder}").Split('\n');

            foreach (var d in directories)
            {
                if (!Directory.Exists($"{folder}/{d}"))
                    Directory.CreateDirectory($"{folder}/{d}");

                if (d != "")
                    CheckFilesToUpdate($"{folder}/{d}");
            }
        }

        public void StartUpdateGame()
        {
            CheckFilesToUpdate($"../Files/{Name}");

            timerUpdateProgress = new Timer(UpdateProgressBar, null, 1000, 1000);
            foreach (var path in FilesToUpdate.Keys)
            {
                DownloadFile(path, 0, FilesToUpdate[path]);
            }

            timerUpdateProgress.Dispose();
            CurrentStatus = Status.Ready;
            File.WriteAllText($"Files/{Name}/data.txt", Status.Ready.ToString());
            UpdateVersion();
        }

        private void DownloadFolder(string path)
        {
            string[] files = web.DownloadString($"{ServerAddress}getFiles.php?Folder={path}/").Split('\n');

            
            foreach (var f in files)
            {
                if (f == "")
                    continue;

                long startByte = 0;
                string filePath = $"{path}/{f}";
                if (File.Exists(filePath)) {

                    using (var stream = File.OpenRead(filePath))
                        startByte = stream.Length;
                }

                long size = Int64.Parse(web.DownloadString($"{ServerAddress}fileSize.php?Path={path}/{f}"));

                if (size == startByte)
                    continue;


                if (f == "version")
                    continue;


                DownloadFile($"{path}/{f}", startByte, size);
            }

            string[] directories = web.DownloadString($"{ServerAddress}getFolders.php?Folder={path}").Split('\n');

            foreach (var d in directories)
            {
                if (!Directory.Exists($"{path}/{d}"))
                    Directory.CreateDirectory($"{path}/{d}");

                if(d != "")
                DownloadFolder($"{path}/{d}");
            }
        }

        private void DownloadFile(string path, long startByte, long size)
        {
            bool finish = false;
            string result = string.Empty;
            HttpWebRequest request;
            long finishByte = startByte + BufferSize;
            CurrentFile = path;
            WebResponse response;
            byte[] buffer = new byte[BufferSize];

            do
            {
                if (finishByte >= size)
                {
           
                    finishByte = size;
               
                    finish = true;
                }

                byte[] data = web.DownloadData($"{ServerAddress}getFile.php?Path={path}&Start={startByte}&End={finishByte}");

                try { 
                

                    using (FileStream stream = File.Open(path, FileMode.Append))
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    DownloadedSize += BufferSize;
                    CurrentSize += BufferSize;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    continue;
                }
        
                startByte += BufferSize;
                finishByte += BufferSize;      

            } while (!finish);                  
        }

        private long CalculateSize(string directoryPath) =>  
          Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
             .Sum(t => (new FileInfo(t).Length));

        private void UpdateVersion()
        {
            string version = web.DownloadString($"{ServerAddress}fileTimestamp.php?Path=Files/{Name}/version");
            File.WriteAllText($"../Files/{Name}/version", version);
        }

        private void UpdateProgressBar(object state)
        {
            DownloadSpeed = Convert.ToDouble(DownloadedSize/(1024*2));
            DownloadedSize = 0;
            Percentage = Convert.ToInt32(((double)CurrentSize) / ((double)AllSize) * 100); 
        }


        private void OnPropertyChanged(string caller)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
