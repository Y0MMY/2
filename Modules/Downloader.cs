using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace launher
{
    public class Downloader : INotifyPropertyChanged
    {

        private WebClient web;
        private Timer timerUpdateProgress;
        private Timer timerUpdateSpeed;
        private long speedCounter = 0;
        private string currentFile;
        private double downloadSpeed;
        private long currentSize;
        private long allSize;
        bool downloading = false;
        private int percentage;
        string speedLabel;
        string sizeLabel;
        private long writedSize = 0;
        string allSizeLabel;
        private  List<GameFile> filesToUpdate = new List<GameFile>(); 
        private  ConcurrentQueue<GameFile> filesToWrite = new ConcurrentQueue<GameFile>();
        private string downloadLabel;
        private BytesToGiga bytesToGiga = new BytesToGiga();
        private BytesToMega bytesToMega = new BytesToMega();
        private long sizeToDownload;
        public enum Status
        {
            Downloading,
            Updating,
            Ready,
            Writing,
            UpdatingLauncher
        };

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler PercentageUpdated;

        public string ServerAddress { get; set; }
        public string UserFolder { get; set; }
        public string ServerFolder { get; set; }
        public int BufferSize { get; set; } = 1000000;
        public long DownloadedSize { get; set; }
        public Status CurrentStatus { get; set; }


        public string DownloadLabel
        {
            get => downloadLabel;
            set
            {
                downloadLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadLabel)));
            }
        }

        public string SpeedLabel
        {
            get => speedLabel;
            set
            {
                speedLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedLabel)));
            }
        }

        public string SizeLabel
        {
            get => sizeLabel;
            set
            {
                sizeLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeLabel)));
            }
        }

        public string AllSizeLabel
        {
            get => allSizeLabel;
            set
            {
                allSizeLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllSizeLabel)));
            }
        }


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

        private void OnPropertyChanged(string caller)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }


        public Downloader(string server, string serverFolder, string userFolder)
        {
            if (server == null || serverFolder == null)
                throw new Exception("Fields must not be null");           

            if (File.Exists($"{UserFolder}/filesToUpdate"))
            {
                filesToUpdate = JsonConvert.DeserializeObject<List<GameFile>>(File.ReadAllText($"{UserFolder}/filesToUpdate"));
            }

            ServerAddress = server;
            ServerFolder = serverFolder;
            UserFolder = userFolder;
        
            web = new WebClient() { BaseAddress = $"{ServerAddress}" };

            if (userFolder != null)
                CurrentSize = CalculateSize(UserFolder);

            AllSize = ServerFolderSize(ServerFolder);
            sizeToDownload = AllSize * 2;

      

            if (userFolder != null)
            {
                if (File.Exists($"{UserFolder}/data.txt"))
                    CurrentStatus = (Status)Enum.Parse(typeof(Status), File.ReadAllText($"{UserFolder}/data.txt"));
            }
        }

        public async void DownloadGame()
        {
            if(File.Exists($"{UserFolder}/data.txt"))
               CurrentStatus = (Status)Enum.Parse(typeof(Status), File.ReadAllText($"{UserFolder}/data.txt"));
            else
            {
                CurrentStatus = Status.Downloading;
                File.WriteAllText($"{UserFolder}/data.txt", Status.Downloading.ToString());
            }

            if (CurrentStatus != Status.Downloading)
                return;


            CurrentSize = CalculateSize(UserFolder) - CalculateSize(UserFolder + "/Mods");
            AllSize = ServerFolderSize(ServerFolder);


            if (AllSize / 1024 / 1024 / 1024 == 0)
                AllSizeLabel = "мб";
            else
                AllSizeLabel = "гб";

           
            timerUpdateProgress = new Timer(UpdateProgress, null, 0, 200);
            timerUpdateSpeed = new Timer(UpdateSpeed, null, 0, 1000);
            Task t = Task.Run(WriteFile);
            await DownloadFolder(ServerFolder, UserFolder);
            timerUpdateSpeed.Dispose();
            DownloadLabel = "Запись на диск..";

            CurrentStatus = Status.Writing;

            t.Wait();
            timerUpdateProgress.Dispose();
           
            File.WriteAllText($"{UserFolder}/data.txt", Status.Ready.ToString());


            CurrentStatus = Status.Ready;
            SizeLabel = "";
            CurrentFile = "";
            Percentage = 100;
           
            DownloadLabel = "Загрузка завершена";
            PercentageUpdated(this ,null);
        }

        private void UpdateSpeed(object state)
        {
            if (CurrentStatus == Status.Ready)
                return;

            DownloadLabel = "";
            DownloadSpeed = speedCounter;
            speedCounter = 0;

            if ((DownloadSpeed + 1) / 1024 / 1024 < 1)
                SpeedLabel = "кб/с";
            else
                SpeedLabel = "мб/с";

            if (CurrentStatus == Status.Downloading)
                DownloadLabel += "Загрузка игры";
            else if (CurrentStatus == Status.Updating)
                DownloadLabel += "Обновление игры";
            else if (CurrentStatus == Status.UpdatingLauncher)
                DownloadLabel += "Обновление лаунчера";


            DownloadLabel += $" ({bytesToMega.Convert(DownloadSpeed, null, null, null)} {SpeedLabel})";
        }

        private async Task<bool> DownloadFolder(string serverFolder, string userFolder)
        {

            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            foreach(string file in GetFilesServer(serverFolder))
            {
                if(file == "" || file == "data.txt" || file == "version")
                    continue;
           

                string userPath = userFolder + "/" + file;
                string serverPath = serverFolder + "/" + file;
                long startByte = 0;
                long allSize = Int64.Parse(web.DownloadString($"/fileSize.php?Path={serverPath}"));

                if (File.Exists(userPath))
                {
                    FileInfo info = new FileInfo(userPath);
                    startByte = info.Length;
                }

                if (startByte == allSize)
                    continue;

           
               await DownloadFile(serverPath, userPath, startByte, allSize);
            }


            foreach(string folder in GetSubFoldersServer(serverFolder))
            {
                if (folder == "")
                    continue;

               await DownloadFolder($"{serverFolder}/{folder}", $"{userFolder}/{folder}");
            }

            return true;
        }

        public async Task<bool> DownloadFile(string serverPath, string userPath, long startByte, long allSize)
        {
    
            bool isFinish = false;
            long fromByte = startByte ;
            long toByte = startByte + BufferSize;
            byte[] buffer = new byte[BufferSize];
            CurrentFile = userPath;
            while (!isFinish) {

                try
                {

                    if (toByte >= allSize)
                    {
                        toByte = allSize;
                        isFinish = true;
                    }

                    HttpClient http = new HttpClient();
                    http.DefaultRequestHeaders.Range = new RangeHeaderValue(fromByte, toByte - 1);
                    buffer = new byte[toByte - fromByte];

                    using (var responseStream = await http.GetStreamAsync($"{ServerAddress}/{serverPath}"))
                    {
                        using (MemoryStream stream = new MemoryStream(buffer))
                           await responseStream.CopyToAsync(stream);

                        filesToWrite.Enqueue(new GameFile() { UserPath = userPath, bytes = buffer});
                        
                    }

                    DownloadedSize += toByte - fromByte;
                    speedCounter += toByte - fromByte;
                    toByte += BufferSize;
                    fromByte += BufferSize;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    continue;
                }
            }

            return true;
        }

   
        public async void DownloadModAsync(string fileName, long allSize)
        {
            CurrentSize = CalculateSize(UserFolder);
            AllSize = allSize;
            if (AllSize / 1024 / 1024 / 1024 == 0)
                AllSizeLabel = "мб";
            else
                AllSizeLabel = "гб";


            CurrentStatus = Status.Downloading;
            timerUpdateProgress = new Timer(UpdateProgress, null, 0, 200);
            timerUpdateSpeed = new Timer(UpdateSpeed, null, 0, 1000);
            Task t = Task.Run(WriteFile);
            DownloadLabel = "Загрузка мода";

            await DownloadFile(fileName, UserFolder+"/"+fileName.Substring(fileName.LastIndexOf("/") + 1), CurrentSize, AllSize);

            CurrentStatus = Status.Ready;

            DownloadLabel = "Подготовка файлов..";

            timerUpdateProgress.Dispose();
            timerUpdateSpeed.Dispose();

            t.Wait();
    
            File.WriteAllText($"{UserFolder}/data.txt", Status.Ready.ToString());

            DownloadLabel = "Загрузка завершена";
            SizeLabel = "";
            CurrentFile = "";
            Percentage = 100;
           
        }


        private async Task WriteFile()
        {
            GameFile file;

            while (CurrentStatus != Status.Ready || filesToWrite.Count > 0)
            {
                if (CurrentStatus == Status.Writing && filesToWrite.Count == 0)
                    return;

                try
                {
                    if (!filesToWrite.TryDequeue(out file))
                        continue;


                    byte[] arr = file.bytes;

                    using (FileStream stream = File.Open(file.UserPath, FileMode.Append))
                        stream.Write(arr, 0, arr.Length);

                    writedSize += arr.Length;
                }
                catch
                {
                    continue;
                }
            }        
        }

        public bool CheckGameFiles()
        {
            allFiles = CountFiles(UserFolder);
            CheckFilesToUpdate(ServerFolder, UserFolder);
     
            return filesToUpdate.Count > 0;
        }

        public bool CheckLauncherFiles()
        {
            allFiles = CountFiles("./");
            CheckFilesToUpdate(ServerFolder, UserFolder, true);

            return filesToUpdate.Count > 0;
        }

        private void CheckFilesToUpdate(string serverFolder, string userFolder, bool updateLauncher = false)
        {
            if (userFolder == UserFolder + $"/{Constants.ModsFolder}")
                return;


           
            string[] files = GetFilesServer(serverFolder);
            byte[] buffer;
            MD5 hasher = MD5.Create();
            string fileHash;

            foreach (var fi in Directory.GetFiles(userFolder))
            {
                string f;


                if (fi.Contains("\\"))
                    f = fi.Substring(fi.LastIndexOf("\\") + 1);
                else
                    f = fi.Substring(fi.LastIndexOf("/") + 1);


                if (f == "version" || f == "," || f == "" || f == "data.txt" || f == "path" || f == "server")
                    continue;

                if (files.Contains(f) == false)
                {
                    FileInfo info = new FileInfo(fi);
                    CurrentSize -= info.Length;
                    File.Delete(fi);
                }
            }

            foreach (var f in files)
            {

                if (f == "version" || f == "," || f == "" || f == "data.txt" || f == "path")
                    continue;

                string userPath = $"{userFolder}/{f}";
                string serverPath = $"{serverFolder}/{f}";
                if (userFolder == "./")
                    userPath = "./" + f;
           
        

                GameFile check = filesToUpdate.FirstOrDefault(fo => fo.UserPath == userPath);

                if (check != null)
                {
                    FileInfo info = new FileInfo(userPath);
                    check.CurrentFileSize = info.Length;
                    continue;
                }
               

                if (!File.Exists(userPath))
                {
                    filesToUpdate.Add(new GameFile() { ServerPath = serverPath, UserPath = userPath, ServerFileSize = ServerFileSize(serverPath), CurrentFileSize = 0 });
                    continue;
                }

  

                string serverHash = HashServerFile(serverPath);

                using (var stream = File.OpenRead(userPath))
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    buffer = hasher.ComputeHash(buffer);

                    StringBuilder result = new StringBuilder();

                    for (int i = 0; i < buffer.Length; i++)
                        result.Append(buffer[i].ToString("x2"));

                    fileHash = result.ToString();
                }

                if (updateLauncher)
                {
                    userPath += "__upd";
                }

                if (serverHash != fileHash)
                {
                    FileInfo info = new FileInfo(userPath.Replace("__upd", ""));

                    filesToUpdate.Add(new GameFile() { ServerPath = serverPath, UserPath = userPath, ServerFileSize = ServerFileSize(serverPath),
                        CurrentFileSize=0 });

                    if (!updateLauncher && CurrentStatus != Status.Downloading)
                    {
                        CurrentSize -= info.Length;
                        File.Delete(userPath);                    
                    }
                    else
                    {
                        CurrentSize -= info.Length;
                    }
                }

                countFiles++;

                if (!updateLauncher)
                {
                    Percentage = Convert.ToInt32((countFiles / allFiles) * 100);
                    PercentageUpdated?.Invoke(this, null);
                }
            }


            string[] directories = GetSubFoldersServer(serverFolder);

            foreach (var d in directories)
            {
                if (!Directory.Exists($"{userFolder}/{d}"))
                    Directory.CreateDirectory($"{userFolder}/{d}");

                if (d != "")
                {
                    string newDirectory;
                    if(userFolder == "./")
                    {
                        newDirectory = d;
                    }
                    else
                    {
                        newDirectory = $"{userFolder}/{d}";
                    }

                    CheckFilesToUpdate($"{serverFolder}/{d}",newDirectory);
                }
            }
        }

        public async Task StartUpdateLauncher()
        {
           
         
            if (filesToUpdate.Count == 0)
                return;

            CurrentStatus = Status.UpdatingLauncher;
            DownloadLabel = "Обновление лаунчера";
            File.WriteAllText($"./data.txt", Status.Updating.ToString());
            File.WriteAllText($"./filesToUpdate", JsonConvert.SerializeObject(filesToUpdate));

            timerUpdateProgress = new Timer(UpdateProgress, null, 0, 200);
            timerUpdateSpeed = new Timer(UpdateSpeed, null, 0, 1000);
            Task t = Task.Run(WriteFile);
            foreach (var file in filesToUpdate)
            {
                await DownloadFile(file.ServerPath, file.UserPath, file.CurrentFileSize, file.ServerFileSize);
            }
            timerUpdateSpeed.Dispose();
            DownloadLabel = "Запись на диск..";
            CurrentStatus = Status.Writing;
            t.Wait();
         
            timerUpdateProgress.Dispose();

            Percentage = 100;
            PercentageUpdated(this, null);
            CurrentStatus = Status.Ready;


            File.WriteAllText($"./data.txt", Status.Ready.ToString());
            File.Delete($"./filesToUpdate");
            UpdateVersion();
            
            Process.Start("Updater.exe", $"-v {File.ReadAllText("./version")} -i {Process.GetCurrentProcess().Id}");

            await Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                Application.Current.Shutdown();
            });
        
        }
        double allFiles = 0;
        double countFiles = 0;
        public async void StartUpdateGame()
        {
            DownloadLabel = "Проверка файлов";

            allFiles = CountFiles(UserFolder);


            CheckFilesToUpdate(ServerFolder, UserFolder);
            countFiles = 0;

            if (filesToUpdate.Count == 0)
            {
                DownloadLabel = "Обновление завершено";
                SizeLabel = "";
                CurrentFile = "";
                Percentage = 100;
                PercentageUpdated(this, null);
                return;

            }
            UpdateSpeed(null);
            CurrentStatus = Status.Updating;
            File.WriteAllText($"{UserFolder}/data.txt", Status.Updating.ToString());
            File.WriteAllText($"{UserFolder}/filesToUpdate", JsonConvert.SerializeObject(filesToUpdate));

            timerUpdateProgress = new Timer(UpdateProgress, null, 0, 200);
            timerUpdateSpeed = new Timer(UpdateSpeed, null, 0, 1000);
            Task t = Task.Run(WriteFile);
            foreach (var file in filesToUpdate)
            {
               await DownloadFile(file.ServerPath, file.UserPath, file.CurrentFileSize, file.ServerFileSize);
            }

            timerUpdateSpeed.Dispose();
            DownloadLabel = "Запись на диск..";

            CurrentStatus = Status.Writing;

            t.Wait();


            CurrentStatus = Status.Ready;
            timerUpdateProgress.Dispose();
          
            File.WriteAllText($"{UserFolder}/data.txt", Status.Ready.ToString());
            File.Delete($"{UserFolder}/filesToUpdate");

            DownloadLabel = "Обновление завершено";
            SizeLabel = "";
            CurrentFile = "";


            Percentage = 100;
            PercentageUpdated(this, null);
            UpdateVersion();
        }

        private void UpdateVersion()
        {
            string version = web.DownloadString($"/fileTimestamp.php?Path={ServerFolder}/version");
            File.WriteAllText($"{UserFolder}/version", version);
        }

        private void UpdateProgress(object state)
        {
            string sizeL;
            string allsizeL;
            CurrentSize += DownloadedSize;  
            DownloadedSize = 0;
            double csize = (double)CurrentSize + writedSize;
            double asize = (double)sizeToDownload;
            Percentage = Convert.ToInt32(csize / asize * 100);

            PercentageUpdated?.Invoke(this, null);

            if (CurrentSize / 1024 / 1024 / 1024 == 0)
                sizeL = "мб";
            else
                sizeL = "гб";


            if (AllSize / 1024 / 1024 / 1024 == 0)
                allsizeL = "мб";
            else
                allsizeL = "гб";

            SizeLabel = $"{bytesToGiga.Convert(CurrentSize, null, null, null)} {sizeL} из" +
                $" {bytesToGiga.Convert(AllSize, null, null, null)} {allsizeL}";
        
        }

        private long CalculateSize(string directoryPath)
        {
            if (directoryPath == null)
                return 0;

            if (!Directory.Exists($"{UserFolder}/{Constants.ModsFolder}"))
                Directory.CreateDirectory($"{UserFolder}/{Constants.ModsFolder}");


          return  Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Where(f => f != "data.txt" || f.Substring(f.Length-3) != "asi")
               .Sum(t => (new FileInfo(t).Length));

        }


        private long ServerFolderSize(string folder)
        {
           return Int64.Parse(web.DownloadString($"/folderSize.php?Folder={folder}"));
        }

        private long ServerFileSize(string file)
        {
            return Int64.Parse(web.DownloadString($"/fileSize.php?Path={file}")); ;
        }

        private string[] GetFilesServer(string folder)
        {
            return web.DownloadString($"/getFiles.php?Folder={folder}/").Split('\n');
        }

        private string HashServerFile(string file)
        {
            return web.DownloadString($"/hashFile.php?Path={file}");
        }

        private string[] GetSubFoldersServer(string folder)
        {
           return web.DownloadString($"/getFolders.php?Folder={folder}").Split('\n');
        }

        private int CountFiles(string folder)
        {
            return Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Where(f => f != "data.txt").Count();
        }
    }
}
