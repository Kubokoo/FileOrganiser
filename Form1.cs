using System;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Media;
using System.Threading;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;

namespace Files_v3_GUI_
{

    public partial class Main : Form
    {
        public class Files
        {
            public string TargetDir { get; set; }
            public string ScanDir { get; set; }
            public string Name { get; set; }
            public int LZeros { get; set; }
            public string FileType { get; set; }
            public Files(string targetDir, string scanDir, string name, int lZeros, string fileType)
            {
                TargetDir = targetDir;
                ScanDir = scanDir;
                Name = name;
                LZeros = lZeros;
                FileType = fileType;
            }
        }
        Files file;

        public class Extensions
        {
            public int Png { get; set; }
            public int Jpg { get; set; }
            public Extensions(int png, int jpg)
            {
                Png = png;
                Jpg = jpg;
            }
        }
        Extensions extension;
        public string fPath()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                return folderBrowserDialog1.SelectedPath;
            }
            else return null;
        }

        public void add2cb(ComboBox cb, string name)
        {
            if (name != null)
            {
                cb.Items.Add(name);
                cb.SelectedIndex = cb.Items.Count - 1;
            }
        }

        bool validationE = false;
        void Error(string eType,ComboBox source)
        {
            SystemSounds.Beep.Play();
            validationE = true;
            switch (eType)
            {
                case "dir":
                    {
                        eType = "Directory you entered does't exists.";
                        break;
                    }
                case "lZeroE":
                    {
                        eType = "Filed \"Leading zeros: \" doesn't contain number.";
                        break;
                    }
                case "name":
                    {
                        eType = "Field name is empty or only contains charters which can't be ussed as a name";
                        break;
                    }
                case "no number":
                    {
                        eType = "Ther was no number in settings.json file in your target localizaion.";
                        break;
                    }
                case "don't create file":
                    {
                        eType = "This program can't operate without settings.json file.";
                        break;
                    }
            }
            MessageBox.Show(eType);
            SearchStopping();
            ErrorProvider cbError = errorProvider1;
            cbError.SetError(source, eType);
        }

        static string validateInput(string value)
        {
            char[] chrasToTrim = { '*', ' ', '/', '?', '<', '>', '|' };
            return value.Trim(chrasToTrim);
        }

        string validateDir(string dir,ComboBox source)
        {
            if (dir != "\\" && Directory.Exists(dir) == true) return dir;
            else
            {
                Error("dir",source);
                return "Error@#$%*^%&";
            }
        }

        public bool validate()
        {
            //Adding text to comboboxes
            if (StargetDir != true) add2cb(comboBox1, comboBox1.Text);
            if (SscanDir != true) add2cb(comboBox2, comboBox2.Text);
            add2cb(comboBox3, comboBox3.Text);
            add2cb(comboBox4, comboBox4.Text);

            //Validating Input
            string targetDir = comboBox1.Text.ToString();
            if (!targetDir.EndsWith(@"\")) targetDir = targetDir + @"\";
            targetDir = validateInput(targetDir);
            if (validateDir(targetDir, comboBox1) == "Error@#$%*^%&") return false;

            string scanDir = comboBox2.Text.ToString();
            if (scanDir.EndsWith(@"\")) scanDir = scanDir.Remove(scanDir.Length - 1, 1);
            scanDir = validateInput(scanDir);
            if(validateDir(scanDir, comboBox2) == "Error@#$%*^%&") return false;

            string name = comboBox3.Text.ToString();
            name = validateInput(name);
            if (name == "")
            {
                Error("name", comboBox3);
                return false;
            }

            string lZeroS = comboBox4.Text.ToString();
            lZeroS = Regex.Match(lZeroS, @"\d+").Value;
            int lZero = -1;
            try
            {
                lZero = Convert.ToInt16(lZeroS);
            }
            catch (Exception)
            {
                Error("lZeroE", comboBox4);
            }

            if (validationE == false && lZero != -1)
            {
                file = new Files(targetDir, scanDir, name, lZero, "");
                return true;
            }
            else return false;
        }

        void controlsD()
        {
            if (start == true)
            {
                tableLayoutPanel1.Enabled = false;
                lbLog.Enabled = true;
            } 
            else tableLayoutPanel1.Enabled = true;
        }

        void ReadJson()
        {
            try
            {
                extension = JsonConvert.DeserializeObject<Extensions>(File.ReadAllText(file.TargetDir + "settings.json"));
            }
            catch (FileNotFoundException)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to create file setting.json ?", "Missing settings.json", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    File.WriteAllText(file.TargetDir + "settings.json", "{\"Png\":0,\"Jpg\":0}");
                    extension = new Extensions(0, 0);
                }
                else if (dialogResult == DialogResult.No)
                {
                    Error("don't create file",comboBox1);
                }
            }
        }

        async void SaveJson(string fileType, string TargetDir)
        {
            if (fileType == "png") extension.Png = extension.Png + 1;
            if (fileType == "jpg") extension.Jpg = extension.Jpg + 1;
            bool resault = await Task.Run(() => SaveJsonA(fileType));

        }

        public bool SaveJsonA(string fileType)
        {
            try{
                File.WriteAllText(file.TargetDir + "settings.json", JsonConvert.SerializeObject(extension));
            }
            catch (IOException)
            {
                Thread.Sleep(100);
                File.WriteAllText(file.TargetDir + "settings.json", JsonConvert.SerializeObject(extension));
            }
            //File.WriteAllText(file.TargetDir + "settings.json", JsonConvert.SerializeObject(extension));
            return true;
        }

        public void MovingFilesEvent(string ScanedElement)
        {
            if (file != null)
            {
                file.FileType = Path.GetExtension(ScanedElement);
                file.FileType = file.FileType.Remove(0, 1);
                if (file.FileType == "jpg" | file.FileType == "png")
                {
                    int i1 = -1;
                    if (file.FileType == "jpg") i1 = extension.Jpg;
                    if (file.FileType == "png") i1 = extension.Png;
                    Loop:
                    try
                    {
                        File.Move(ScanedElement, file.TargetDir + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType);
                    }
                    catch (IOException)
                    {
                        i1++;
                        goto Loop;
                    }
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add(DateTime.Now.ToString("HH:mm:ss ") + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType); }));
                    //MessageBox.Show(file.TargetDir + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType);
                    SaveJson(file.FileType, file.TargetDir);
                }
            }
        }
        public void MovingFiles()
        {
            var destination = Directory.GetFiles(file.ScanDir, "*.*", SearchOption.TopDirectoryOnly);//SearchOption.AllDirectories
            foreach (var element in destination)
            {
                file.FileType = Path.GetExtension(element);
                file.FileType = file.FileType.Remove(0, 1);
                if (file.FileType == "jpg" | file.FileType == "png")
                {
                    int i1 = -1;
                    if (file.FileType == "jpg") i1 = extension.Jpg;
                    if (file.FileType == "png") i1 = extension.Png;
                    lbLog.Invoke(new MethodInvoker(delegate { lbLog.Items.Add(DateTime.Now.ToString("HH:mm:ss ") + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType); }));
                    //MessageBox.Show(file.TargetDir + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType);
                    File.Move(element, file.TargetDir + file.Name + " (" + i1.ToString("D" + file.LZeros.ToString()) + ")." + file.FileType);
                    SaveJson(file.FileType, file.TargetDir);

                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void FileWatcher()
        {
            if (file != null)
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.Path = file.ScanDir;
                watcher.Created += new FileSystemEventHandler(Fileready);
                watcher.EnableRaisingEvents = true;
                if (start == false) watcher.Dispose();
            }
        }
        private void Fileready(object source, FileSystemEventArgs e)
        {
            MovingFilesEvent(e.FullPath);
        }

        void ProgrssBarLoop()
        {
            System.Timers.Timer restart = new System.Timers.Timer();
            restart.Elapsed += new ElapsedEventHandler(RestartPBEvent);
            restart.Interval = 300;
            restart.Enabled = true;
            if (start == false)
            {
                progressBar1.Value = 0;
                restart.Dispose();
            }
        }

        private void RestartPBEvent(object source, ElapsedEventArgs e)//TODO Uzupełnianie paska do 100
        {
            if (start == true)
            {
                if (progressBar1.InvokeRequired)
                {
                    progressBar1.Invoke(new Action(() =>
                    {
                        if (progressBar1.Value >= 100)
                        {
                            progressBar1.Value = 0;
                        }
                        progressBar1.Value += 20;
                    }));
                }
            }
            else
            {
                if (progressBar1.InvokeRequired)
                {
                    progressBar1.Invoke(new Action(() =>
                    {
                        progressBar1.Value = 0;
                    }));
                }
            }

        }


        public Main()
        {
            InitializeComponent();
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            comboBox1.Leave += new EventHandler(AutoScanEvent);
            
        }

        void SearchList(List<string> list, string text, ComboBox target)
        {
            int listLength = list.Count,i = 0;
            for (; i < listLength; i++) if (list[i] == text) break;
            if(i>= listLength)
            {
                add2cb(target, text);
                list.Add(text);
            }
        }

        List<string> NameList = new List<string>();
        List<string> LeadingZerosList = new List<string>();
        private void AutoScanEvent(object sender, EventArgs e)
        {
            string dir = comboBox1.Text;
            if (Directory.Exists(dir))
            {
                var destination = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var element in destination)
                {
                    var extension = Path.GetExtension(element);
                    extension = extension.Remove(0, 1);
                    if (extension == "jpg" | extension == "png")
                    {
                        var filename = Path.GetFileName(element);
                        int filenameLength = filename.Length;
                        int extensionLength = extension.Length;
                        filename = filename.Remove(filenameLength - extensionLength, extensionLength);
                        int startindex = filename.IndexOf('(');
                        if (startindex != -1)
                        {
                            int leadingZeros = filename.Substring(startindex, filenameLength - startindex - extensionLength).Length-3;
                            string name = filename.Remove(startindex - 1);
                            SearchList(NameList, name, comboBox3);
                            SearchList(LeadingZerosList, leadingZeros.ToString(), comboBox4);
                        }
                    }
                }
            }
        }

        bool StargetDir = false;
        private void button2_Click(object sender, EventArgs e) //Target dir
        {
            add2cb(comboBox1, fPath());
            StargetDir = true;
            AutoScanEvent(sender, e);
        }

        bool SscanDir = false;
        private void button3_Click(object sender, EventArgs e)//Scan dir
        {
            add2cb(comboBox2, fPath());
            SscanDir = true;
        }

        bool start = false;
        Thread Progres;
        private void ST_Click(object sender, EventArgs e)
        {
            if (start == false) //lbLog.BackColor = Color.FromArgb(189,189,189);//lbLog.DoDragDrop(data:)
            {
                start = true;
                ST.Text = "Stop";
                controlsD();
                if (validate() == true)
                {
                    Progres = new Thread(new ThreadStart(ProgrssBarLoop));
                    Progres.Start();
                    ReadJson();
                    FileWatcher();
                    MovingFiles();
                    //TODO Drag&Drop
                    //TODO Saving enterered text and restoring it
                }
            }
            else
            {
                SearchStopping();
            }
        }
        void SearchStopping()
        {
            if (Progres != null && Progres.ThreadState == ThreadState.Running) Progres.Abort();
            //if (AutoScan.ThreadState == ThreadState.Running) AutoScan.Abort();
            start = false;
            ST.Text = "Start";
            controlsD();
            ProgrssBarLoop();
            FileWatcher();
            SscanDir = false;
            StargetDir = false;
            validationE = false;
            file = null;
            extension = null;

        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
