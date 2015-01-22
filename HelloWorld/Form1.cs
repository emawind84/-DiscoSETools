using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Threading;

namespace DiscoSETools
{

    public partial class Form1 : Form
    {

        // Define static variables shared by class methods. 
        

        public Form1()
        {
            InitializeComponent();
        }

        


        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        

        /// <summary>
        /// MAIN FORM LOAD METHOD!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Load(object sender, EventArgs e)
        {
            UpdateServicesStatus();

            dailyArchiveDate.Value = DateTime.Today;

            var backupPrefixNameList = Properties.Settings.Default.BackupPrefixNameList;
            StringEnumerator benum = backupPrefixNameList.GetEnumerator();
            while (benum.MoveNext())
            {
                backupPrefixNameTextBox.AppendText(benum.Current + Environment.NewLine);
            }

            passPortServiceNameTextBox.Text = Properties.Settings.Default.PassPortServiceName;
            scriptFolderPathTextBox.Text = Properties.Settings.Default.ScriptDirPath;
            archiveSaveFolderPathTextBox.Text = Properties.Settings.Default.ArchiveSaveFolderPath;
            backupFolderPathTextBox.Text = Properties.Settings.Default.BackupFolderPath;
            backupTaskNameTextBox.Text = Properties.Settings.Default.BackupTaskName;

            PrintProductVersion();
        }

        /// <summary>
        /// SERVICE STATUS UPDATE
        /// </summary>
        private void UpdateServicesStatus()
        {
            string status = Utils.StatusService(Properties.Settings.Default.PassPortServiceName);
            Console.WriteLine("Service status: " + status);
            passPortServiceLabel.Text = status;
            creativeServerStatusLabel.Text = Utils.StatusService("discoworld");
            survivalServerStatusLabel.Text = Utils.StatusService("discoworld_survival");
        }

        /// <summary>
        /// MANUAL BACKUP SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackupButtonClick(object sender, EventArgs e)
        {
            string scriptDir = Properties.Settings.Default.ScriptDirPath;

            var utils = new Utils();
            utils.ExecuteCommand("cmd", String.Format("/C {0}\\backup.bat \"{1}\"", scriptDir, scriptDir));
            consoleResultTextBox.Text = utils.Output;
        }

        /// <summary>
        /// DAILY ARCHIVE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DailyArchiveButtonClick(object sender, EventArgs e)
        {
            DateTime dt = dailyArchiveDate.Value;
            string dts = dt.ToString("yyyy-MM-dd");

            StringCollection bks = Properties.Settings.Default.BackupPrefixNameList;
            string backupFolderPath = Properties.Settings.Default.BackupFolderPath;
            string archiveFolderPath = Properties.Settings.Default.ArchiveSaveFolderPath;
            string scriptDir = Properties.Settings.Default.ScriptDirPath;

            ThreadPool.SetMaxThreads(1, 1);

            for (var i = 0; i < bks.Count; i++)
            {
                if ( String.IsNullOrEmpty(bks[i]) ) continue;

                ExecuteCommandThread thrd = new ExecuteCommandThread{
                    CommandWithArgs = String.Format("/C {0}/daily_archive.bat {1} \"{2}\" \"{3}\" \"{4}\"", scriptDir, dts, bks[i], backupFolderPath, archiveFolderPath )
                };
                thrd.CommandExecuted += WriteResultOnConsole;

                ThreadPool.QueueUserWorkItem(thrd.ThreadPoolCallback, i);

                //Thread oThread = new Thread(new ThreadStart(thrd.Execute) );
                //oThread.Start();

            }
        }

        /// <summary>
        /// DISABLE BACKUP TASK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableBackupTaskButtonClick(object sender, EventArgs e)
        {
            string backupTaskName = Properties.Settings.Default.BackupTaskName;
            var utils = new Utils();
            utils.ExecuteCommand("cmd", String.Format( "/C {0}\\disable_task_backup.bat \"{1}\"", Properties.Settings.Default.ScriptDirPath, backupTaskName ) );
            consoleResultTextBox.Text = utils.Output;
        }

        /// <summary>
        /// ENABLE BACKUP TASK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableBackupTaskButtonClick(object sender, EventArgs e)
        {
            string backupTaskName = Properties.Settings.Default.BackupTaskName;
            var utils = new Utils();
            utils.ExecuteCommand("cmd", String.Format( "/C {0}\\enable_task_backup.bat \"{1}\"", Properties.Settings.Default.ScriptDirPath, backupTaskName) );
            consoleResultTextBox.Text = utils.Output;
        }

        /// <summary>
        /// START PASSPORT SERVICE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartPPServiceButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread
            {
                ServiceName = Properties.Settings.Default.PassPortServiceName,
                Timeout = 5000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Start));
            // Start the thread
            oThread.Start();
        }

        /// <summary>
        /// STOP PASSPORT SERVICE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopPPServiceButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread { 
                ServiceName = Properties.Settings.Default.PassPortServiceName,
                Timeout = 5000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Stop));
            // Start the thread
            oThread.Start();

            // Spin for a while waiting for the started thread to become
            // alive:
            //while (!oThread.IsAlive) ;

            //Utils.StopService(Properties.Settings.Default.PassPortServiceName, 5000);
            //UpdateServicesStatus();
        }

        /// <summary>
        /// SAVE SETTINGS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsSaveButtonClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.ScriptDirPath = scriptFolderPathTextBox.Text;
            Properties.Settings.Default.PassPortServiceName = passPortServiceNameTextBox.Text;
            Properties.Settings.Default.ArchiveSaveFolderPath = archiveSaveFolderPathTextBox.Text;
            Properties.Settings.Default.BackupFolderPath = backupFolderPathTextBox.Text;
            Properties.Settings.Default.BackupTaskName = backupTaskNameTextBox.Text;

            // save backup prefix name list
            SaveBackupPrefixName();

            // save settings
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();

        }

        private void SaveBackupPrefixName()
        {
            Properties.Settings.Default.BackupPrefixNameList.Clear();
            string[] lines = backupPrefixNameTextBox.Lines;
            for (var i = 0; i < lines.Length; i++)
            {
                Console.WriteLine("Adding new backup prefix item...");
                Properties.Settings.Default.BackupPrefixNameList.Add(lines[i]);
            }
        }

        private void ScriptFolderPathButtonClick(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                scriptFolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void BackupFolderPathButtonClick(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                backupFolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void ArchiveSaveFolderPathButtonClick(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                archiveSaveFolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void MainTabPageClick(object sender, EventArgs e)
        {
            UpdateServicesStatus();
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PrintProductVersion()
        {
            versionLabel.Text = "Version: " + Application.ProductVersion;
        }

        /// <summary>
        /// STATUS CREATIVE SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreativeStatusButtonClick(object sender, EventArgs e)
        {
            consoleResultTextBox.AppendText(Utils.GetSteamServerInfo("203.239.21.69:27016", WriteResultOnConsole));
        }

        /// <summary>
        /// STATUS SURVIVAL SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SurvivalStatusButtonClick(object sender, EventArgs e)
        {
            consoleResultTextBox.AppendText(Utils.GetSteamServerInfo("203.239.21.69:27017", WriteResultOnConsole));
        }

        /// <summary>
        /// STOP CREATIVE SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopCreativeButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread
            {
                ServiceName = "discoworld",
                Timeout = 20000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Stop));
            // Start the thread
            oThread.Start();

        }

        /// <summary>
        /// START CREATIVE SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartCreativeButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread
            {
                ServiceName = "discoworld",
                Timeout = 20000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Start));
            // Start the thread
            oThread.Start();
        }

        /// <summary>
        /// STOP SURVIVAL SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopSurvivalButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread
            {
                ServiceName = "discoworld_survival",
                Timeout = 20000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Stop));
            // Start the thread
            oThread.Start();
        }

        /// <summary>
        /// START SURVIVAL SERVER
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSurvivalButtonClick(object sender, EventArgs e)
        {
            ServiceThread srvt = new ServiceThread
            {
                ServiceName = "discoworld_survival",
                Timeout = 20000
            };
            srvt.CompletedEvent += WriteResultOnConsole;
            Thread oThread = new Thread(new ThreadStart(srvt.Start));
            // Start the thread
            oThread.Start();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// TIMER FOR SERVICE STATUS UPDATE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerStatusTimerTick(object sender, EventArgs e)
        {
            string dts = DateTime.Now.ToString("MM-dd HH:mm:ss");

            SetConsoleTextBox(String.Format("[{0}] Server status updated", dts));
            SetConsoleTextBox(Environment.NewLine);
            
            UpdateServicesStatus();
        }

        /// <summary>
        /// HANDLER FOR WRITING OUTPUT TO FORM CONSOLE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="output"></param>
        void WriteResultOnConsole(object sender, string output)
        {
            string dts = DateTime.Now.ToString("MM-dd HH:mm:ss");
            SetConsoleTextBox( String.Format("[{0}]{1}", dts, Environment.NewLine));
            if (!String.IsNullOrEmpty(output)) {
                //Console.WriteLine(output);
                SetConsoleTextBox(output + Environment.NewLine);
            }
        }

        delegate void SetConsoleTextBoxCallback(string text);

        private void SetConsoleTextBox(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.consoleResultTextBox.InvokeRequired)
            {
                SetConsoleTextBoxCallback d = new SetConsoleTextBoxCallback(SetConsoleTextBox);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.consoleResultTextBox.AppendText( text );
            }
        }

        private void ConsoleClearButtonClick(object sender, EventArgs e)
        {
            consoleResultTextBox.Clear();
        }

    }

}
