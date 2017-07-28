using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using SDInstallTool.Detection;
using SDInstallTool.Helpers;
using System.Drawing;

namespace SDInstallTool
{
    public partial class MainForm : Form
    {
        #region Constructor

        private const String registryKey = "SOFTWARE\\MiSTer Project\\SDCardInstaller";

        #endregion

        #region Fields

        private static MainForm _instance;

        private DriveDetector _driveWatcher;
        private bool isUpdatePackageValid = false;
        private bool isDiskUpdatable = false;
        private String currentPhysicalDisk = String.Empty;

        private LogForm frmLog = null;

        #endregion

        #region Constructor / Initialization

        public MainForm()
        {
            InitializeComponent();
            _instance = this;

            MessageBoxEx.Owner = this.Handle;


            ResetProgress();
            CheckUpdatePackage();

            // Set version into title
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Text += @" v" + version;

            // Set app icon (not working on Mono/Linux)
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                Icon = Utility.GetAppIcon();

            PopulateDrives();
            if (comboBoxDrives.Items.Count > 0)
            {
                EnableButtons();
            }
            else
            {
                DisableButtons(false);
            }

            // Read registry values
            var key = Registry.LocalMachine.CreateSubKey(registryKey);
            if (key != null)
            {
                var file = (string)key.GetValue("FileName", "");
                if (File.Exists(file))
                    textBoxFileName.Text = file;

                var drive = (string)key.GetValue("Drive", "");
                if (string.IsNullOrEmpty(drive))
                {
                    foreach(var cbDrive in comboBoxDrives.Items)
                    {
                        var item = (ComboBoxItem)cbDrive;
                        if(item.Name == drive)
                        {
                            comboBoxDrives.SelectedItem = cbDrive;
                        }
                    }
                }

                key.Close();
            }

            // Detect insertions / removals
            StartListenForChanges();
        }

        #endregion

        #region Properties

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #endregion Properties

        #region Disk access event handlers

        /// <summary>
        /// Called to update progress bar as we read/write disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progressPercentage"></param>
        void _disk_OnProgress(object sender, int progressPercentage)
        {
            progressBar.Value = progressPercentage;
            Application.DoEvents();
        }

        /// <summary>
        /// Called to display/log messages from disk handling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        void _disk_OnLogMsg(object sender, string message)
        {
            labelStatus.Text = message;
            Application.DoEvents();
        }

        #endregion

        #region UI events handling

        /// <summary>
        /// Close the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonEraseMBRClick(object sender, EventArgs e)
        {
            if (comboBoxDrives.SelectedIndex < 0)
                return;

            var drive = (string)comboBoxDrives.SelectedItem;

            var success = false;
            try
            {
            }
            catch (Exception ex)
            {
                success = false;
                labelStatus.Text = ex.Message;
            }

            if (!success)
                MessageBoxEx.Show("Problem writing to disk. Is it write-protected?", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBoxEx.Show("MBR erased. Please remove and reinsert to format", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        /// <summary>
        /// Called to persist registry values on closure so we can remember things like last file used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            var key = Registry.LocalMachine.CreateSubKey(registryKey);
            if (key != null)
            {
                key.SetValue("FileName", textBoxFileName.Text);
                key.Close();
            }

            StopListenForChanges();
        }

        #endregion

        #region UI logic implementation

        /// <summary>
        /// Load in the drives
        /// </summary>
        private void PopulateDrives()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(PopulateDrives));
                return;
            }

            comboBoxDrives.SelectedIndex = -1;
            comboBoxDrives.Items.Clear();
            ComboBoxItem selectedItem = null;

            try
            {
                var removableDisks = DiskManagement.getRemovableDisks();
                foreach (var disk in removableDisks)
                {
                    var item = new ComboBoxItem(disk.displayName, disk.physicalName);

                    // If disk name matches previously used one - remember the item
                    if (disk.physicalName.Equals(this.currentPhysicalDisk))
                    {
                        selectedItem = item;
                    }

                    comboBoxDrives.Items.Add(item);
                }
            }
            catch
            {
                Logger.Error("Unable to get list of removable disks");
            }

            // Restore item selection
            if (comboBoxDrives.Items.Count > 0)
            {
                if (selectedItem != null)
                {
                    comboBoxDrives.SelectedItem = selectedItem;
                }
                else
                {
                    comboBoxDrives.SelectedIndex = 0;
                }
            }
            else
            {
                textDiskSize.Text = String.Empty;
            }
        }

        /// <summary>
        /// Callback when removable media is inserted or removed, repopulates the drive list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void driveWatcherEvent(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(driveWatcherEvent));
                return;
            }

            //TODO: Remove Debug
            Logger.Info("driveWatcherEvent()");

            PopulateDrives();

            if (comboBoxDrives.Items.Count > 0)
                EnableButtons();
            else
                DisableButtons(false);
        }

        /// <summary>
        /// Updates UI to disable buttons
        /// </summary>
        /// <param name="running">Whether read/write process is running</param>
        private void DisableButtons(bool running)
        {
            comboBoxDrives.Enabled = false;
            textBoxFileName.Enabled = false;

            buttonRefresh.Enabled = false;

            buttonFull.Enabled = false;
            buttonLinux.Enabled = false;
            buttonWipe.Enabled = false;

            textDiskSize.Enabled = false;
        }

        /// <summary>
        /// Updates UI to enable buttons
        /// </summary>
        private void EnableButtons()
        {
            comboBoxDrives.Enabled = true;
            textBoxFileName.Enabled = true;

            buttonRefresh.Enabled = true;

            // Enable buttons base on conditions
            buttonFull.Enabled = isUpdatePackageValid;
            buttonLinux.Enabled = isUpdatePackageValid && isDiskUpdatable;

            buttonWipe.Enabled = true;
            textDiskSize.Enabled = true;

            ResetProgressAfterDelay();
        }

        private void ShowMessageBoxTopmost(String message, String title)
        {
            #region Ensure executed in main thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    ShowMessageBoxTopmost(message, title);
                });

                return;
            }
            #endregion Ensure executed in main thread

            Form topmostForm = new Form();
            // We do not want anyone to see this window so position it off the
            // visible screen and make it as small as possible
            topmostForm.Size = new Size(1, 1);
            topmostForm.StartPosition = FormStartPosition.Manual;
            Rectangle rect = SystemInformation.VirtualScreen;
            topmostForm.Location = new Point(rect.Bottom + 10, rect.Right + 10);
            topmostForm.Show();

            // Make this form the active form and make it TopMost
            topmostForm.Focus();
            topmostForm.BringToFront();
            topmostForm.TopMost = true;

            // Show the MessageBox with the form just created as its owner
            DialogResult result = MessageBox.Show(topmostForm, message, title);

            topmostForm.Dispose();
        }

        #endregion UI logic implementation

        #region Business Logic

        void CheckUpdatePackage()
        {
            //TODO: Remove Debug
            Logger.Info("CheckUpdatePackage()...");

            var error = ImageManager.checkUpdatePackage();
            var statusText = string.Empty;
            var nameText = string.Empty;

            #region Check for mandatory packages
            if (!String.IsNullOrEmpty(error))
            {
                // Update package is invalid
                isUpdatePackageValid = false;

                nameText = "Update package incorrect";
            }
            else
            {
                // Success
                isUpdatePackageValid = true;

                nameText = "U-Boot + Linux";
            }

            if (isUpdatePackageValid)
            {
                statusText = @"Update package found";
            }
            else
            {
                statusText = @"Update package invalid";
            }
            #endregion Check for mandatory packages

            #region Check for optional MiSTer app package
            if (ImageManager.checkMisterPackage())
            {
                nameText += " + MiSTer";
            }
            #endregion

            textBoxFileName.Text = nameText;
            labelStatus.Text = statusText;

            //TODO: Remove Debug
            Logger.Info("done");
        }

        /// <summary>
        /// Check if currently selected disk is compatible with partial update
        /// </summary>
        /// <returns></returns>
        bool CheckPartialUpdateCompatible(String physicalDiskName, uint bytesPerSector)
        {
            bool result = false;

            //TODO: Remove Debug
            Logger.Info("CheckPartialUpdateCompatible()...");

            try
            {
                result = DiskManagement.CheckDiskCompatible(physicalDiskName, bytesPerSector);
            }
            catch (Exception e)
            {
                Logger.Error("Unable to check card for MiSTer compatibility");
                Logger.Error(e.Message);
            }

            isDiskUpdatable = result;

            // Update UI status
            if (isDiskUpdatable)
            {
                buttonLinux.Enabled = true;
            }
            else
            {
                buttonLinux.Enabled = false;
            }

            //TODO: Remove Debug
            Logger.Info("done");

            return result;
        }

        void DiskOperationStarted()
        {
            labelStatus.Text = "Processing...";

            // Don't refresh drive statuses until disk operation(s) finished
            StopListenForChanges();

            // Block all UI controls
            DisableButtons(true);
        }

        void DiskOperationFinished()
        {
            labelStatus.Text = "Done";

            // Start listening for drive statuses again
            StartListenForChanges();

            // Refresh drives list
            PopulateDrives();

            // Unlock user
            EnableButtons();
        }

        #endregion

        #region Disk Change Handling

        public bool StartListenForChanges()
        {
            if (_driveWatcher == null)
            {
                _driveWatcher = new DriveDetector();
            }

            _driveWatcher.DeviceArrived += OnDriveConnected;
            _driveWatcher.DeviceRemoved += OnDriveRemoved;

            return true;
        }

        public void StopListenForChanges()
        {
            if (_driveWatcher != null)
            {
                _driveWatcher.DeviceArrived -= OnDriveConnected;
                _driveWatcher.DeviceRemoved -= OnDriveRemoved;
            }
        }

        void OnDriveConnected(object sender, DriveDetectorEventArgs e)
        {
            // Run in worked thread to prevent COM-related errors (WMI calls)
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                driveWatcherEvent(sender, e);
            };

            worker.RunWorkerAsync();
        }

        void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            // Run in worked thread to prevent COM-related errors (WMI calls)
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                driveWatcherEvent(sender, e);
            };

            worker.RunWorkerAsync();
        }

        #endregion

        #region Control events

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            CheckUpdatePackage();

            PopulateDrives();
        }

        private void buttonFull_Click(object sender, EventArgs e)
        {
            var item = (ComboBoxItem)this.comboBoxDrives.SelectedItem;

            if (item != null && item.value.Length > 0)
            {
                var disk = DiskManagement.getDiskDescriptor(item.value);

                var message = string.Format("All data on '{0} - {1}' will be lost.\r\n Do you want to continue?", disk.displayName, disk.description);
                var confirmResult = MessageBox.Show(message, "Warning!", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    DiskOperationStarted();

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        if (DiskManagement.fullInstall(disk.physicalName))
                        {
                            message = string.Format("Full install for disk {0} finished successfully", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation finished");
                        }
                        else
                        {
                            message = string.Format("Unable to install on {0}", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation failed");
                        }
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        DiskOperationFinished();
                        CheckPartialUpdateCompatible(disk.physicalName, disk.bytesPerSector);
                    };

                    worker.RunWorkerAsync();
                }
            }
        }

        private void buttonLinux_Click(object sender, EventArgs e)
        {
            var item = (ComboBoxItem)this.comboBoxDrives.SelectedItem;

            if (item != null && item.value.Length > 0)
            {
                var disk = DiskManagement.getDiskDescriptor(item.value);

                var message = string.Format("MiSTer system partitions will be updated.\r\nUser data will remain untouched and safe\r\n Do you want to continue?", disk.displayName, disk.description);
                var confirmResult = MessageBox.Show(message, "Warning!", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    DiskOperationStarted();

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        if (DiskManagement.updateLinux(disk.physicalName))
                        {
                            message = string.Format("System update on disk {0} finished successfully", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation finished");
                        }
                        else
                        {
                            message = string.Format("Unable to update on {0}", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation failed");
                        }
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        DiskOperationFinished();
                        CheckPartialUpdateCompatible(disk.physicalName, disk.bytesPerSector);
                    };

                    worker.RunWorkerAsync();
                }
            }
        }

        private void buttonWipe_Click(object sender, EventArgs e)
        {
            var item = (ComboBoxItem)this.comboBoxDrives.SelectedItem;

            if (item != null && item.value.Length > 0)
            {
                var disk = DiskManagement.getDiskDescriptor(item.value);

                var message = string.Format("All data on '{0} - {1}' will be lost.\r\n Do you want to continue?", disk.displayName, disk.description);
                var confirmResult = MessageBox.Show(message, "Warning!", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    DiskOperationStarted();

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += delegate
                    {
                        if (DiskManagement.wipeDisk(disk.physicalName))
                        {
                            SetProgress(100);
                            message = string.Format("{0} wiped successfully", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation finished");
                        }
                        else
                        {
                            SetProgress(100);
                            message = string.Format("Unable to wipe {0}", disk.displayName);
                            ShowMessageBoxTopmost(message, "Operation failed");
                        }
                    };
                    worker.RunWorkerCompleted += delegate
                    {
                        DiskOperationFinished();
                        CheckPartialUpdateCompatible(disk.physicalName, disk.bytesPerSector);
                    };

                    worker.RunWorkerAsync();
                }
                else
                {
                    // If 'No', do something here.
                }
            }
        }

        private void driveSelectionChanged(object sender, EventArgs e)
        {
            var idx = comboBoxDrives.SelectedIndex;
            var item = (ComboBoxItem)comboBoxDrives.SelectedItem;

            if (item != null && item.value.Length > 0)
            {
                var diskDescriptor = DiskManagement.getDiskDescriptor(item.value);

                // Remember current selection
                this.currentPhysicalDisk = diskDescriptor.physicalName;

                textDiskSize.Text = diskDescriptor.description;

                // Check drive
                CheckPartialUpdateCompatible(diskDescriptor.physicalName, diskDescriptor.bytesPerSector);
            }
            else
            {
                textDiskSize.Text = String.Empty;
            }
        }

        private void showLogWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmLog == null || frmLog.IsDisposed)
            {
                frmLog = new LogForm();
            }

            if (frmLog.Visible)
            {
                frmLog.Hide();
            }
            else
            {
                frmLog.Show();
            }
        }

        #endregion

        #region Progress

        public void SetProgressValue(int value)
        {
            #region Ensure executed in main thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    SetProgressValue(value);
                });

                return;
            }
            #endregion Ensure executed in main thread

            if (value < 0)
                value = 0;
            if (value > 100)
                value = 100;

             progressBar.Value = value;
        }

        public static void SetProgress(int value)
        {
            if (value < 0)
                value = 0;
            if (value > 100)
                value = 100;

            if (_instance != null)
            {
                _instance.SetProgressValue(value);
            }
        }

        public static void ResetProgress()
        {
            if (_instance != null)
            {
                _instance.SetProgressValue(0);
                _instance.SetStatsValue("");
            }
        }

        public static void ResetProgressAfterDelay()
        {
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((obj) =>
            {
                ResetProgress();

                timer.Dispose();
            },
            null, 1000, System.Threading.Timeout.Infinite);
        }

        #endregion Progress

        #region Stats

        private void SetStatsValue(String value)
        {
            #region Ensure executed in main thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    SetStatsValue(value);
                });

                return;
            }
            #endregion Ensure executed in main thread

            labelStats.Text = value;
        }

        public static void SetStats(String value)
        {
            if (_instance != null)
            {
                _instance.SetStatsValue(value);
            }
        }

        #endregion Stats

        #region Logging

        public static void LogMessage(String message)
        {
            if (_instance != null && _instance.frmLog != null && !_instance.frmLog.IsDisposed)
            {
                _instance?.frmLog?.logMessageLine(message);
            }
        }

        #endregion Logging
    }
}
