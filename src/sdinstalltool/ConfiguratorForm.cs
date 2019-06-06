using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MiSTerConfigurator
{
    public partial class ConfiguratorForm : Form
    {
        const String strMiSTerURL = "https://github.com/MiSTer-devel/Main_MiSTer";
        const int intErrorPause = 1000;
        String strWorkDir = System.IO.Path.Combine("Scripts", ".mister_updater");
        bool blnInitialized = false;
        System.Net.WebClient objWebClient = new System.Net.WebClient();
        bool blnDownloadingCores = false;
        bool blnDownloadingExtras = false;
        bool blnRunningWizard = false;
        String strMiSTerINI_GitHub = "";
        String strMiSTerINI_Current = "";

        String strMiSTerDir_constructor = "";
        bool blnMiSTerDir_locked = false;

        enum enmOS : byte { Windows, MacOS, Linux };
        enmOS bytOS = enmOS.Windows;
 
        public ConfiguratorForm()
        {
            InitializeComponent();
        }

        public ConfiguratorForm(String MiSTerDir, bool locked)
        {
            strMiSTerDir_constructor = MiSTerDir;
            blnMiSTerDir_locked = locked;
            InitializeComponent();
        }

        private void ConfiguratorForm_Load(object sender, EventArgs e)
        {
            const System.Security.Authentication.SslProtocols _Tls12 = (System.Security.Authentication.SslProtocols)0x00000C00; const System.Net.SecurityProtocolType Tls12 = (System.Net.SecurityProtocolType)_Tls12; System.Net.ServicePointManager.SecurityProtocol = Tls12;
            // hideTabHeaders();
            enableAdvancedMode(false);
            
            if (isMacOSX()) bytOS = enmOS.MacOS;
            else if (Environment.OSVersion.Platform == PlatformID.Unix) bytOS = enmOS.Linux;
            else bytOS = enmOS.Windows;
            switch (bytOS) {
                case enmOS.MacOS:
                    linkLabelUnixInstaller.Text = "macOS MiSTer SD Card Formatter Script (by michaelshmitty)";
                    linkLabelUnixInstaller.Tag = "https://github.com/michaelshmitty/SD-Installer-macos_MiSTer|MiSTer-sd-installer-macos.sh";
                    linkLabelUnixInstaller.Visible = true;
                    break;
                case enmOS.Linux:
                    linkLabelUnixInstaller.Text = "Linux MiSTer SD Card Formatter Script (by alanswx)";
                    linkLabelUnixInstaller.Tag = "https://github.com/alanswx/SD-installer_MiSTer|create_sd.sh";
                    linkLabelUnixInstaller.Visible = true;
                    testLinuxCertificates();
                    break;
                default:
                    linkLabelUnixInstaller.Visible = false;
                    break;
            };

            timer1.Enabled = true;

            if (strMiSTerDir_constructor != "") {
                cmbMiSTerDir.Items.Add(strMiSTerDir_constructor);
            };
            if (blnMiSTerDir_locked) {
                cmbMiSTerDir.Enabled = false;
            }
            else {
            cmbMiSTerDir.Items.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "MiSTer"));
            cmbMiSTerDir.SelectedIndex = cmbMiSTerDir.Items.Count - 1;
                foreach (System.IO.DriveInfo objDrive in System.IO.DriveInfo.GetDrives())
                {
                    if (objDrive.DriveType == System.IO.DriveType.Removable && objDrive.IsReady) {
                        cmbMiSTerDir.Items.Add(objDrive.RootDirectory.Name);
                        if (System.IO.File.Exists(System.IO.Path.Combine(objDrive.RootDirectory.Name, "MiSTer")) && System.IO.File.Exists(System.IO.Path.Combine(objDrive.RootDirectory.Name, "menu.rbf")) && System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(objDrive.RootDirectory.Name, "linux"), "linux.img")))
                        {
                            cmbMiSTerDir.SelectedIndex = cmbMiSTerDir.Items.Count-1;
                        };
                    };
                };
            };
            if (strMiSTerDir_constructor != "") {
                cmbMiSTerDir.SelectedIndex=0;
            };
        }

        private bool isMacOSX () {
            String strUname="";

            System.Diagnostics.Process objProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "uname",
                    Arguments = "-s"
                }
            };
            try
            {
                objProcess.Start();
                strUname = objProcess.StandardOutput.ReadToEnd().Trim();
            }
            catch { };
            objProcess.Dispose();

            if (strUname == "Darwin")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool testLinuxCertificates()
        {
            String strCertBundle = "";
            System.Diagnostics.Process objProcess;

            try {
                objWebClient.DownloadString("https://google.com");
                return true;
            }
            catch
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.StartupPath, "cert-sync")))
                {
                    if (System.IO.File.Exists("/etc/ssl/certs/ca-certificates.crt")) {
                        strCertBundle = "/etc/ssl/certs/ca-certificates.crt";
                    }
                    else if (System.IO.File.Exists("/etc/pki/tls/certs/ca-bundle.crt")) {
                        strCertBundle = "/etc/pki/tls/certs/ca-bundle.crt";
                    };
                    if (strCertBundle!="") {
                        objProcess = new System.Diagnostics.Process
                        {
                            StartInfo =
                            {
                                UseShellExecute = false,
                                RedirectStandardOutput = false,
                                FileName = System.IO.Path.Combine(Application.StartupPath, "cert-sync"),
                                Arguments = "--user " + strCertBundle
                            }
                        };
                        try {
                            objProcess.Start();
                            objProcess.WaitForExit();
                        }
                        catch { };
                        try {
                            objWebClient.DownloadString("https://google.com");
                            return true;
                        }
                        catch { };
                    };
                }
            };
            return false;
        }

        private void readSamba()
        {
            String strFileContent;
            if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir),"linux"),"samba.sh"))) {
                chkEnableSamba.Checked = true;
                chkEnableSamba.Enabled = true;
                strFileContent = readFileSTR(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "samba.sh"));
            }
            else if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_samba.sh")))
            {
                chkEnableSamba.Checked = false;
                chkEnableSamba.Enabled = true;
                strFileContent = readFileSTR(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_samba.sh"));
            }
            else
            {
                chkEnableSamba.Checked = false;
                chkEnableSamba.Enabled = false;
                strFileContent = "";
            };
            if (strFileContent != "") {
                txtSambaUserName.Text = getINIValueSTR(strFileContent, "SAMBA_USER");
                txtSambaPassword.Text = getINIValueSTR(strFileContent, "SAMBA_PASS");
            };
        }

        private void readWiFi()
        {
            String strFileContent;
            if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "wpa_supplicant.conf")))
            {
                chkEnableWiFi.Checked = true;
                chkEnableWiFi.Enabled = true;
                strFileContent = readFileSTR(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "wpa_supplicant.conf"));
            }
            else if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_wpa_supplicant.conf")))
            {
                chkEnableWiFi.Checked = false;
                chkEnableWiFi.Enabled = true;
                strFileContent = readFileSTR(System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_wpa_supplicant.conf"));
            }
            else
            {
                chkEnableWiFi.Checked = false;
                chkEnableWiFi.Enabled = false;
                strFileContent = "";
            };
            if (strFileContent != "")
            {
                txtWiFiCountry.Text = getINIValueSTR(strFileContent, "country");
                txtWiFiSSID.Text = getINIQuotedValueSTR(strFileContent, "ssid");
                txtWiFiPassword.Text = getINIQuotedValueSTR(strFileContent, "psk");
            };
        }

        private String readFileSTR(String fileName)
        {
            String strFile = "";
            System.IO.StreamReader objSR = null;
            if (!System.IO.File.Exists(fileName)) return "";
            objSR = new System.IO.StreamReader(fileName);
            strFile = objSR.ReadToEnd();
            objSR.Close();
            return strFile;
        }
        private void writeFileSTR(String fileName, String fileContent)
        {
            System.IO.StreamWriter objSW = null;
            //if (!System.IO.File.Exists(fileName)) return;
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName)) && !CreateDirectorySafe(System.IO.Path.GetDirectoryName(fileName))) return;

            objSW = new System.IO.StreamWriter(fileName);
            objSW.Write(fileContent);
            objSW.Close();
        }
        private void readMiSTerINI(String strMiSTerINI)
        {
            //String strMiSTerINI=readFileSTR(iniFile);
            int intValue;

            if (strMiSTerINI=="") return;

            intValue = getINIValueINT(strMiSTerINI, "video_mode");
            if (intValue >= 0) cmbVideoMode.SelectedIndex = intValue;
            if (getINIValueINT(strMiSTerINI, "video_mode_pal") >= 0 && getINIValueINT(strMiSTerINI, "video_mode_ntsc") >= 0) {
                chkEnablePAL_NTSC.Checked = true;
            }
            else {
                chkEnablePAL_NTSC.Checked = false;
            };
            intValue = getINIValueINT(strMiSTerINI, "vscale_mode");
            if (intValue >= 0) cmbScalingMode.SelectedIndex = intValue;
            intValue = getINIValueINT(strMiSTerINI, "vsync_adjust");
            if (intValue >= 0) cmbVSyncMode.SelectedIndex = intValue;
        }
        private String getINIValueSTR(String INI, String key) {
            System.Text.RegularExpressions.Regex objRegEx = new System.Text.RegularExpressions.Regex("^\\s*" + key + "\\s*=\\s*(?<Value>[a-zA-Z0-9%().,/_-]+)", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Match objMatch = objRegEx.Match(INI);
            if (objMatch.Success)
            {
                return objMatch.Groups["Value"].Value;
            }
            else {
                return "";
            };
        }
        private String getINIQuotedValueSTR(String INI, String key)
        {
            System.Text.RegularExpressions.Regex objRegEx = new System.Text.RegularExpressions.Regex("^\\s*" + key + "\\s*=\\s*\"(?<Value>[^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Match objMatch = objRegEx.Match(INI);
            if (objMatch.Success)
            {
                return objMatch.Groups["Value"].Value;
            }
            else
            {
                return "";
            };
        }
        private int getINIValueINT(String INI, String key)
        {
            String strValue = getINIValueSTR(INI, key);
            int intValue = -1;
            if (strValue != "") {
                int.TryParse(strValue, out intValue);
            };
            return intValue;
        }
        private void saveMiSTerINI(ref String strMiSTerINI, String iniFile)
        {
            //String strMiSTerINI = readFileSTR(iniFile);
                        
            if (strMiSTerINI == "") return;

            strMiSTerINI = setINIValue(strMiSTerINI, "video_mode", cmbVideoMode.SelectedIndex.ToString(), false);
            switch (cmbVideoMode.SelectedIndex)
            {
                case 8:
                case 9:
                    strMiSTerINI = setINIValue(strMiSTerINI, "video_mode_ntsc", 8.ToString(), !chkEnablePAL_NTSC.Checked);
                    strMiSTerINI = setINIValue(strMiSTerINI, "video_mode_pal", 9.ToString(), !chkEnablePAL_NTSC.Checked);
                    break;
                default:
                    strMiSTerINI = setINIValue(strMiSTerINI, "video_mode_ntsc", 0.ToString(), !chkEnablePAL_NTSC.Checked);
                    strMiSTerINI = setINIValue(strMiSTerINI, "video_mode_pal", 7.ToString(), !chkEnablePAL_NTSC.Checked);
                    break;
            }

            strMiSTerINI = setINIValue(strMiSTerINI, "vscale_mode", cmbScalingMode.SelectedIndex.ToString(), false);
            strMiSTerINI = setINIValue(strMiSTerINI, "vsync_adjust", cmbVSyncMode.SelectedIndex.ToString(), false);
            
            writeFileSTR(iniFile, strMiSTerINI);
        }
        private String setINIValue(String INI, String key, String value, bool comment)
        {
            String strOutINI = INI;
            System.Text.RegularExpressions.Regex objRegExUncommented = new System.Text.RegularExpressions.Regex("^\\s*" + key + "\\s*=\\s*(?<Value>[a-zA-Z0-9%().,/_-]+)", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Regex objRegExCommented = new System.Text.RegularExpressions.Regex("^\\s*;\\s*" + key + "\\s*=", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Match objMatch = objRegExUncommented.Match(strOutINI);
            if (!objMatch.Success)
            {
                strOutINI = objRegExCommented.Replace(strOutINI, key + "=", 1);
            };
            strOutINI = objRegExUncommented.Replace(strOutINI, ((comment)?"; ":"") + key + "=" + value, 1);

            return strOutINI;
        }
        private String setINIQuotedValue(String INI, String key, String value, bool comment)
        {
            String strOutINI = INI;
            System.Text.RegularExpressions.Regex objRegExUncommented = new System.Text.RegularExpressions.Regex("^\\s*" + key + "\\s*=\\s*\"(?<Value>[^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Regex objRegExCommented = new System.Text.RegularExpressions.Regex("^\\s*;\\s*" + key + "\\s*=", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Match objMatch = objRegExUncommented.Match(strOutINI);
            if (!objMatch.Success)
            {
                strOutINI = objRegExCommented.Replace(strOutINI, key + "=", 1);
            };
            strOutINI = objRegExUncommented.Replace(strOutINI, ((comment) ? "; " : "") + key + "=\"" + value + "\"", 1);

            return strOutINI;
        }
        private void saveSamba() {
            String strFileSambaEnabled=System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "samba.sh");
            String strFileSambaDisabled=System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_samba.sh");
            String strSamba;

            if (chkEnableSamba.Checked) {
                if (!System.IO.File.Exists(strFileSambaEnabled))
                {
                    if (System.IO.File.Exists(strFileSambaDisabled))
                    {
                        System.IO.File.Move(strFileSambaDisabled, strFileSambaEnabled);
                    };
                }
                if (System.IO.File.Exists(strFileSambaEnabled)) {
                    strSamba = readFileSTR(strFileSambaEnabled);
                    strSamba = setINIValue(strSamba, "SAMBA_USER", txtSambaUserName.Text, false);
                    strSamba = setINIValue(strSamba, "SAMBA_PASS", txtSambaPassword.Text, false);
                    writeFileSTR(strFileSambaEnabled, strSamba);
                };
            }
            else
            {
                if (System.IO.File.Exists(strFileSambaEnabled))
                {
                    if (System.IO.File.Exists(strFileSambaDisabled))
                    {
                        System.IO.File.Delete(strFileSambaEnabled);
                    }
                    else { 
                        System.IO.File.Move(strFileSambaEnabled,strFileSambaDisabled);
                    };
                };
            };
        }
        private void saveWiFi() {
            String strFileWiFiEnabled = System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "wpa_supplicant.conf");
            String strFileWiFiDisabled = System.IO.Path.Combine(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "linux"), "_wpa_supplicant.conf");
            String strWiFi;

            if (chkEnableWiFi.Checked)
            {
                if (!System.IO.File.Exists(strFileWiFiEnabled))
                {
                    if (System.IO.File.Exists(strFileWiFiDisabled))
                    {
                        System.IO.File.Move(strFileWiFiDisabled, strFileWiFiEnabled);
                    };
                }
                if (System.IO.File.Exists(strFileWiFiEnabled))
                {
                    strWiFi = readFileSTR(strFileWiFiEnabled);
                    strWiFi = setINIValue(strWiFi, "country", txtWiFiCountry.Text, false);
                    strWiFi = setINIQuotedValue(strWiFi, "ssid", txtWiFiSSID.Text, false);
                    strWiFi = setINIQuotedValue(strWiFi, "psk", txtWiFiPassword.Text, false);
                    writeFileSTR(strFileWiFiEnabled, strWiFi);
                };
            }
            else
            {
                if (System.IO.File.Exists(strFileWiFiEnabled))
                {
                    if (System.IO.File.Exists(strFileWiFiDisabled))
                    {
                        System.IO.File.Delete(strFileWiFiEnabled);
                    }
                    else
                    {
                        System.IO.File.Move(strFileWiFiEnabled, strFileWiFiDisabled);
                    };
                };
            };
        }

        private void writeLog(String log) {
            System.IO.StreamWriter objSW = new System.IO.StreamWriter(Application.ExecutablePath + ".log",true);
            objSW.WriteLine("{0} - {1}", DateTime.Now.ToString(), log);
            objSW.Flush();
            objSW.Close();
        }

        private void loadCoresTree()
        {
            String strMiSTerWiki = null;
            System.Text.RegularExpressions.Regex objRegEx = new System.Text.RegularExpressions.Regex("(?:(?<CoreURL>https://github.com/[a-zA-Z0-9./_-]*_MiSTer)\">(?<CoreName>.*?)<)|(?:user-content-(?<CoreCategory>[a-z-]*))");
            System.Windows.Forms.TreeNode objCategoryNode = null;

            writeStatusLabel("Loading cores list");
            Application.DoEvents();

            try {
                strMiSTerWiki = objWebClient.DownloadString("https://github.com/MiSTer-devel/Main_MiSTer/wiki/");
            }
            catch (System.Exception ex)
            {
                writeLog(ex.ToString());
                writeStatusLabel("Error downloading MiSTer wiki");
                System.Threading.Thread.Sleep(intErrorPause);
                return;
            };
            strMiSTerWiki = strMiSTerWiki.Substring(strMiSTerWiki.IndexOf("user-content-cores"));
            strMiSTerWiki = strMiSTerWiki.Substring(0, strMiSTerWiki.IndexOf("user-content-development"));
            foreach (System.Text.RegularExpressions.Match objMatch in objRegEx.Matches(strMiSTerWiki))
            {
                if (objMatch.Groups["CoreCategory"].Value != "")
                {
                    objCategoryNode = treeViewCores.Nodes.Find(objMatch.Groups["CoreCategory"].Value, true)[0];
                }
                else
                {
                    if (!objMatch.Groups["CoreURL"].Value.EndsWith("/Menu_MiSTer"))
                    {
                        objCategoryNode.Nodes.Add(objMatch.Groups["CoreName"].Value).Tag = objMatch.Groups["CoreURL"].Value;
                    };
                };
            };
            writeStatusLabel("Ready");
        }

        private void downloadCores(System.Windows.Forms.TreeNodeCollection Nodes, String currentDirectory, String workDirectory) {
            foreach (System.Windows.Forms.TreeNode objNode in Nodes) {
                if (!blnDownloadingCores) return;
                if (objNode.Tag == null || objNode.Tag.ToString() == "")
                {
                    switch (objNode.Name)
                    {
                        case "cores":
                            currentDirectory = System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), getControlText_TS(txtComputerDir));
                            break;
                        case "console-cores":
                            currentDirectory = System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), getControlText_TS(txtConsoleDir));
                            break;
                        case "arcade-cores":
                            currentDirectory = System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), getControlText_TS(txtArcadeDir));
                            break;
                        case "service-cores":
                            currentDirectory = System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), getControlText_TS(txtUtilityDir));
                            break;
                    };
                    downloadCores(objNode.Nodes, currentDirectory, workDirectory);
                }
                else
                {
                    if (objNode.Checked) downloadCore(objNode.Text, objNode.Tag.ToString(), currentDirectory, workDirectory);
                };
            };
        }

        private delegate String delegateGetControlText(System.Windows.Forms.Control control);
        private String getControlText(System.Windows.Forms.Control control) {
            return control.Text;
        }
        private String getControlText_TS(System.Windows.Forms.Control control) {
            return this.Invoke(new delegateGetControlText(getControlText), control).ToString();
        }

        private delegate void delegateDownloadCores(System.Windows.Forms.TreeNodeCollection Nodes, String currentDirectory, String workDirectory);
        private void asyncDownloadCoresCallBack(IAsyncResult AsyncResult)
        {
            ((delegateDownloadCores)AsyncResult.AsyncState).EndInvoke(AsyncResult);
            enableCoresUI_TS(true);
            if (blnRunningWizard) {
                writeStatusLabel("Downloading Extras");
                asyncDownloadExtras(treeViewExtras.Nodes, getControlText_TS(cmbMiSTerDir));
            };
        }
        private void asyncDownloadCores(System.Windows.Forms.TreeNodeCollection Nodes, String currentDirectory, String workDirectory)
        {
            delegateDownloadCores objDelegateDownloadCores = new delegateDownloadCores(downloadCores);
            objDelegateDownloadCores.BeginInvoke(Nodes, currentDirectory, workDirectory, asyncDownloadCoresCallBack, objDelegateDownloadCores);
        }

        private void enableCoresUI(bool enable) {
            btnDownloadCores.Text = (enable) ? "Download" : "Stop";
            treeViewCores.Enabled = enable;
            txtComputerDir.Enabled = enable;
            txtConsoleDir.Enabled = enable;
            txtArcadeDir.Enabled = enable;
            txtUtilityDir.Enabled = enable;
            cmbMiSTerDir.Enabled = enable;
            if (enable) writeStatusLabel("Ready");
            Application.DoEvents();
        }
        private delegate void delegateEnableCoresUI(bool enable);
        private void enableCoresUI_TS(bool enable) {
            this.Invoke(new delegateEnableCoresUI(enableCoresUI), enable);
        }

        private void writeStatusLabel(String labelText) {
            toolStripStatusLabel1.Text = labelText;
            Application.DoEvents();
        }
        private delegate void delegateWriteStatusLabel(String labelText);
        private void writeStatusLabel_TS(String labelText) {
            this.Invoke(new delegateWriteStatusLabel(writeStatusLabel), labelText);
        }

        private void downloadCore(String coreName, String coreURL, String coreDirectory, String workDirectory) {
            String strReleases;
            System.Text.RegularExpressions.Regex objRegExReleasesURL = new System.Text.RegularExpressions.Regex("/MiSTer-devel/[a-zA-Z0-9./_-]*/tree/[a-zA-Z0-9./_-]*/releases");
            System.Text.RegularExpressions.Regex objRegExReleases = new System.Text.RegularExpressions.Regex("/MiSTer-devel/.*/(?<FileName>(?<BaseName>[a-zA-Z0-9._-]*)_(?<TimeStamp>[0-9]{8}[a-zA-Z]?)(?<FileExtension>\\.rbf|\\.rar)?)");
            System.Text.RegularExpressions.Match objMaxReleaseMatch=null;
            System.Text.RegularExpressions.Regex objRegExLocalFiles = null;
            System.Text.RegularExpressions.Match objCurrentLocalFileMatch=null;
            System.Text.RegularExpressions.Match objMaxLocalFileMatch=null;
            String strDestinationFile=null;
            String strDestinationDirectory = null;

            writeStatusLabel_TS("Checking " + coreName);
            Application.DoEvents();
            try {
                strReleases = objWebClient.DownloadString(coreURL);
                strReleases = objRegExReleasesURL.Match(strReleases).Value;
                strReleases = objWebClient.DownloadString("https://github.com" + strReleases.Replace("/tree/", "/file-list/"));
            }
            catch (System.Exception ex) {
                writeLog(ex.ToString());
                writeStatusLabel_TS("Error checking " + coreName);
                System.Threading.Thread.Sleep(intErrorPause);
                return;
            };
            foreach (System.Text.RegularExpressions.Match objMatch in objRegExReleases.Matches(strReleases)) {
                if ((coreName != "Atari 800XL" || objMatch.Groups["BaseName"].Value == "Atari800") && (coreName != "Atari 5200" || objMatch.Groups["BaseName"].Value == "Atari5200"))
                {
                    if (objMaxReleaseMatch == null || objMatch.Groups["TimeStamp"].Value.CompareTo(objMaxReleaseMatch.Groups["TimeStamp"].Value) > 0)
                    {
                        objMaxReleaseMatch = objMatch;
                    };
                };
            };
            if (objMaxReleaseMatch!=null) {
                switch (objMaxReleaseMatch.Groups["BaseName"].Value)
                {
                    case "MiSTer":
                    case "menu":
                        strDestinationDirectory = workDirectory;
                        break;
                    default:
                        strDestinationDirectory = coreDirectory;
                        break;
                };
                if (System.IO.Directory.Exists(strDestinationDirectory))
                {
                    objRegExLocalFiles = new System.Text.RegularExpressions.Regex(objMaxReleaseMatch.Groups["BaseName"].Value + "_(?<TimeStamp>[0-9]{8}[a-zA-Z]?)" + objMaxReleaseMatch.Groups["FileExtension"].Value.Replace(".", "\\.") + "$");
                    foreach (String strFile in System.IO.Directory.GetFiles(strDestinationDirectory, objMaxReleaseMatch.Groups["BaseName"].Value + "*" + objMaxReleaseMatch.Groups["FileExtension"].Value))
                    {
                        objCurrentLocalFileMatch = objRegExLocalFiles.Match(strFile);
                        if (objCurrentLocalFileMatch != null && objCurrentLocalFileMatch.Value != "")
                        {
                            if (objCurrentLocalFileMatch.Groups["TimeStamp"].Value == "") {
                                System.IO.File.Delete(strFile);
                            }
                            else {
                                if (objMaxReleaseMatch.Groups["TimeStamp"].Value.CompareTo(objCurrentLocalFileMatch.Groups["TimeStamp"].Value) > 0)
                                {
                                    System.IO.File.Delete(strFile);
                                };
                                if (objMaxLocalFileMatch == null || objCurrentLocalFileMatch.Groups["TimeStamp"].Value.CompareTo(objMaxLocalFileMatch.Groups["TimeStamp"].Value) > 0)
                                {
                                    objMaxLocalFileMatch = objCurrentLocalFileMatch;
                                };                            
                            };
                        };
                    };
                };
                if (objMaxLocalFileMatch==null || objMaxReleaseMatch.Groups["TimeStamp"].Value.CompareTo(objMaxLocalFileMatch.Groups["TimeStamp"].Value)>0) {
                    if (!System.IO.Directory.Exists(strDestinationDirectory) && !CreateDirectorySafe(strDestinationDirectory)) return;

                    strDestinationFile = System.IO.Path.Combine(strDestinationDirectory, objMaxReleaseMatch.Groups["FileName"].Value);
                    writeStatusLabel_TS("Downloading " + coreName);
                    Application.DoEvents();
                    try
                    {
                        objWebClient.DownloadFile("https://github.com" + objMaxReleaseMatch.Value + "?raw=true", strDestinationFile);
                    }
                    catch (System.Exception ex) {
                        writeLog(ex.ToString());
                        writeStatusLabel_TS("Error downloading " + coreName);
                        System.Threading.Thread.Sleep(intErrorPause);
                        return;
                    };
                    switch (objMaxReleaseMatch.Groups["BaseName"].Value)
                    {
                        case "MiSTer":
                            if (System.IO.File.Exists(System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value))) System.IO.File.Delete(System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value));
                            System.IO.File.Move(strDestinationFile, System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value));
                            createEmptyFile(strDestinationFile);
                            break;
                        case "menu":
                            if (System.IO.File.Exists(System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value + objMaxReleaseMatch.Groups["FileExtension"].Value))) System.IO.File.Delete(System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value + objMaxReleaseMatch.Groups["FileExtension"].Value));
                            System.IO.File.Move(strDestinationFile, System.IO.Path.Combine(coreDirectory, objMaxReleaseMatch.Groups["BaseName"].Value + objMaxReleaseMatch.Groups["FileExtension"].Value));
                            createEmptyFile(strDestinationFile);
                            break;
                    };
                };
            };
        }

        private void downloadExtras(System.Windows.Forms.TreeNodeCollection Nodes, String currentDir)
        {
            String[] strExtraOptions=null;
            foreach (System.Windows.Forms.TreeNode objNode in Nodes)
            {
                if (!blnDownloadingExtras) return;
                if (objNode.Checked) {
                    strExtraOptions = objNode.Tag.ToString().Split('|');
                    switch (objNode.Name)
                    {
                        case "Cheats":
                            downloadCheats(strExtraOptions[0], strExtraOptions[1], System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strExtraOptions[2]), System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
                            break;
                        default:
                            if (objNode.Tag != null && objNode.Tag.ToString() != "")
                            {
                                downoadExtra(objNode.Text, strExtraOptions[0], strExtraOptions[1], System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strExtraOptions[2]));
                            };
                            break;
                    };
                };
            };
        }

        private delegate void delegateDownloadExtras(System.Windows.Forms.TreeNodeCollection Nodes, String currentDir);
        private void asyncDownloadExtrasCallBack(IAsyncResult AsyncResult)
        {
            ((delegateDownloadExtras)AsyncResult.AsyncState).EndInvoke(AsyncResult);
            enableExtrasUI_TS(true);
            if (blnRunningWizard) enableWizardUI_TS(true);
        }
        private void asyncDownloadExtras(System.Windows.Forms.TreeNodeCollection Nodes, String currentDir)
        {
            delegateDownloadExtras objDelegateDownloadExtras = new delegateDownloadExtras(downloadExtras);
            objDelegateDownloadExtras.BeginInvoke(Nodes, currentDir, asyncDownloadExtrasCallBack, objDelegateDownloadExtras);
        }

        private void downoadExtra(String extraName, String extraURL, String extraFilters, String extraDirectory) {
            String strReleases;
            System.Text.RegularExpressions.Regex objRegExReleases = new System.Text.RegularExpressions.Regex("href=\"(?<ExtraURL>[^\"]*/(?<ExtraFile>[^\"]*?(?:" + extraFilters.Replace(" ", "|") + ")))\".*?<td class=\"age\">.*?<time-ago datetime=\"(?<Year>\\d{4})-(?<Month>\\d{2})-(?<Day>\\d{2})T(?<Hour>\\d{2}):(?<Minute>\\d{2}):(?<Second>\\d{2})Z\">", System.Text.RegularExpressions.RegexOptions.Singleline);
            String strLocalFileName;
            DateTime dtmReleaseDateTimeUTC;

            writeStatusLabel_TS("Checking " + extraName);
            extraURL = extraURL.Replace("/tree/master/", "/file-list/master/");
            if (!extraURL.Contains("/file-list/master")) extraURL = extraURL + "/file-list/master";
            try
            {
                strReleases = objWebClient.DownloadString(extraURL);
            }
            catch (System.Exception ex)
            {
                writeLog(ex.ToString());
                writeStatusLabel_TS("Error checking " + extraName);
                System.Threading.Thread.Sleep(intErrorPause);
                return;
            };
            foreach (System.Text.RegularExpressions.Match objMatch in objRegExReleases.Matches(strReleases)) {
                if (!blnDownloadingExtras) return;
                strLocalFileName = System.IO.Path.Combine(extraDirectory, objMatch.Groups["ExtraFile"].Value);
                dtmReleaseDateTimeUTC = new DateTime(int.Parse(objMatch.Groups["Year"].Value), int.Parse(objMatch.Groups["Month"].Value), int.Parse(objMatch.Groups["Day"].Value), int.Parse(objMatch.Groups["Hour"].Value), int.Parse(objMatch.Groups["Minute"].Value), int.Parse(objMatch.Groups["Second"].Value));
                if (!System.IO.File.Exists(strLocalFileName) || dtmReleaseDateTimeUTC.CompareTo(System.IO.File.GetLastWriteTimeUtc(strLocalFileName))>0  ) {
                    writeStatusLabel_TS("Downloading " + objMatch.Groups["ExtraFile"].Value);
                    if (!System.IO.Directory.Exists(extraDirectory) && !CreateDirectorySafe(extraDirectory)) return;
                    try
                    {
                        objWebClient.DownloadFile("https://github.com" + objMatch.Groups["ExtraURL"].Value + "?raw=true", strLocalFileName);
                    }
                    catch (System.Exception ex)
                    {
                        writeLog(ex.ToString());
                        writeStatusLabel_TS("Error downloading " + objMatch.Groups["ExtraFile"].Value);
                        System.Threading.Thread.Sleep(intErrorPause);
                        return;
                    };
                };
            };
        }

        private void downloadCheats(String cheatsURL, String cheatsMap, String cheatsDirectory, String workDirectory) {
            String strReleases;
            System.Text.RegularExpressions.Regex objRegExReleases = new System.Text.RegularExpressions.Regex("href=\"(?<CheatFile>mister_(?<CheatSystem>[^\"]*)_(?<TimeStamp>\\d{8})\\.zip)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            System.Text.RegularExpressions.Regex objRegExLocalFiles = null;
            System.Text.RegularExpressions.Match objCurrentLocalFileMatch=null;
            System.Text.RegularExpressions.Match objMaxLocalFileMatch=null;
            String strDestinationFile = null;
            String strDestinationDirectory = null;
            // ICSharpCode.SharpZipLib.Zip.FastZip objFastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
            System.Collections.Specialized.StringDictionary objCheatsMap = new System.Collections.Specialized.StringDictionary();

            foreach (String strCheatMap in cheatsMap.Split(' '))
            {
                objCheatsMap.Add(strCheatMap.Substring(0, strCheatMap.IndexOf(':')), strCheatMap.Substring(strCheatMap.IndexOf(':')+1));
            };

            writeStatusLabel_TS("Checking Cheats");
            try {
                strReleases = objWebClient.DownloadString(cheatsURL); 
            }
            catch (System.Exception ex)
            {
                writeLog(ex.ToString());
                writeStatusLabel_TS("Error checking Cheats");
                System.Threading.Thread.Sleep(intErrorPause);
                return;
            };
            foreach (System.Text.RegularExpressions.Match objMatch in objRegExReleases.Matches(strReleases))
            {
                if (objCheatsMap.ContainsKey(objMatch.Groups["CheatSystem"].Value)) {
                    objRegExLocalFiles = new System.Text.RegularExpressions.Regex("(?<CheatFile>mister_" + objMatch.Groups["CheatSystem"].Value + "_(?<TimeStamp>\\d{8})\\.zip)", System.Text.RegularExpressions.RegexOptions.Singleline);
                    objCurrentLocalFileMatch = null;
                    objMaxLocalFileMatch = null;
                    if (System.IO.Directory.Exists(workDirectory)) {
                        foreach (String strFile in System.IO.Directory.GetFiles(workDirectory, "mister_" + objMatch.Groups["CheatSystem"].Value + "*.zip"))
                        {
                            objCurrentLocalFileMatch = objRegExLocalFiles.Match(strFile);
                            if (objCurrentLocalFileMatch != null && objCurrentLocalFileMatch.Value != "")
                            {
                                if (objCurrentLocalFileMatch.Groups["TimeStamp"].Value == "")
                                {
                                    System.IO.File.Delete(strFile);
                                }
                                else
                                {
                                    if (objMatch.Groups["TimeStamp"].Value.CompareTo(objCurrentLocalFileMatch.Groups["TimeStamp"].Value) > 0)
                                    {
                                        System.IO.File.Delete(strFile);
                                    };
                                    if (objMaxLocalFileMatch == null || objCurrentLocalFileMatch.Groups["TimeStamp"].Value.CompareTo(objMaxLocalFileMatch.Groups["TimeStamp"].Value) > 0)
                                    {
                                        objMaxLocalFileMatch = objCurrentLocalFileMatch;
                                    };
                                };
                            };
                        };
                    };
                    if (objMaxLocalFileMatch == null || objMatch.Groups["TimeStamp"].Value.CompareTo(objMaxLocalFileMatch.Groups["TimeStamp"].Value) > 0) {
                        strDestinationFile = System.IO.Path.Combine(workDirectory, objMatch.Groups["CheatFile"].Value);
                        writeStatusLabel_TS("Downloading " + objMatch.Groups["CheatFile"].Value);
                        if (!System.IO.Directory.Exists(workDirectory) && !CreateDirectorySafe(workDirectory)) return;
                        try {
                            objWebClient.DownloadFile(cheatsURL + objMatch.Groups["CheatFile"].Value, strDestinationFile);
                        }
                        catch (System.Exception ex)
                        {
                            writeLog(ex.ToString());
                            writeStatusLabel_TS("Error downloading " + objMatch.Groups["CheatFile"].Value);
                            System.Threading.Thread.Sleep(intErrorPause);
                            return;
                        };
                        writeStatusLabel_TS("Extracting " + objMatch.Groups["CheatFile"].Value);
                        strDestinationDirectory = System.IO.Path.Combine(cheatsDirectory, objCheatsMap[objMatch.Groups["CheatSystem"].Value]);
                        if (!System.IO.Directory.Exists(strDestinationDirectory) && !CreateDirectorySafe(strDestinationDirectory)) return;

                        // objFastZip.ExtractZip(strDestinationFile, strDestinationDirectory, null);
                        ExtractZipFile(strDestinationFile, "", strDestinationDirectory);
                        System.IO.File.Delete(strDestinationFile);
                        createEmptyFile(strDestinationFile);
                    };
                };
            };
        }


        private void createEmptyFile(String fileName) {
            System.IO.FileStream objEmptyFile = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            objEmptyFile.Close();
        }

        private void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            System.Text.RegularExpressions.Regex objRegExNotValidChars = new System.Text.RegularExpressions.Regex("[\\/:*?\"<>|]");
            ICSharpCode.SharpZipLib.Zip.ZipFile zf = null;
            try
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(archiveFilenameIn);
                zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;		// AES encrypted entries are handled automatically
                }
                foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;			// Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];		// 4K is optimum
                    System.IO.Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String originalEntryFileName = entryFileName;
                    entryFileName = objRegExNotValidChars.Replace(entryFileName, "_");
                    if (entryFileName.CompareTo(originalEntryFileName) != 0) System.Diagnostics.Debug.Print("Invalid Zip Entry File Name: ZIP={0}, ENTRY={1}", archiveFilenameIn, originalEntryFileName);
                    String fullZipToPath = System.IO.Path.Combine(outFolder, entryFileName);
                    string directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        System.IO.Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (System.IO.FileStream streamWriter = System.IO.File.Create(fullZipToPath))
                    {
                        ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        private bool CreateDirectorySafe(String directoryName) {
            try
            {
                System.IO.Directory.CreateDirectory(directoryName);
                return true;
            }
            catch (System.Exception ex)
            {
                writeLog(ex.ToString());
                writeStatusLabel_TS("Error creating " + directoryName);
                System.Threading.Thread.Sleep(intErrorPause);
                return false;
            };
        }

        private void enableExtrasUI(bool enable)
        {
            btnDownloadExtras.Text = (enable) ? "Download" : "Stop";
            treeViewExtras.Enabled = enable;
            cmbMiSTerDir.Enabled = enable;
            if (enable) writeStatusLabel("Ready");
            Application.DoEvents();
        }
        private delegate void delegateEnableExtrasUI(bool enable);
        private void enableExtrasUI_TS(bool enable)
        {
            this.Invoke(new delegateEnableExtrasUI(enableExtrasUI), enable);
        }

        private void enableWizardUI(bool enable)
        {
            btn1ClickSetup.Text = (enable) ? "1 Click Setup and Install" : "Stop";
            cmbVideoMode.Enabled = enable;
            chkEnablePAL_NTSC.Enabled = enable;
            cmbScalingMode.Enabled = enable;
            cmbVSyncMode.Enabled = enable;
            btnCompatibilityPreset.Enabled = enable;
            btnOptimalPreset.Enabled = enable;
            chkEnableSamba.Enabled = enable;
            pnlSamba.Enabled = enable;
            chkEnableWiFi.Enabled = enable;
            pnlWiFi.Enabled = enable;
            btnAdvanced.Enabled = enable;
            if (enable) writeStatusLabel("Ready");
            Application.DoEvents();
        }
        private delegate void delegateEnableWizardUI(bool enable);
        private void enableWizardUI_TS(bool enable)
        {
            this.Invoke(new delegateEnableWizardUI(enableWizardUI), enable);
        }

        private void initializeConfigurator() {
            if (!blnInitialized)
            {
                cmbVideoMode.SelectedIndex = 0;
                cmbScalingMode.SelectedIndex = 0;
                cmbVSyncMode.SelectedIndex = 0;

                readSamba();
                readWiFi();

                //blnDownloadingExtras = true;
                //downoadExtra("MiSTer.ini", "https://github.com/MiSTer-devel/Main_MiSTer", "MiSTer.ini", getControlText_TS(cmbMiSTerDir));
                //readMiSTerINI(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "MiSTer.ini"));
                try
                {
                    strMiSTerINI_GitHub = objWebClient.DownloadString("https://github.com/MiSTer-devel/Main_MiSTer/blob/master/MiSTer.ini?raw=true");
                }
                catch (System.Exception ex)
                {
                    writeLog(ex.ToString());
                    writeStatusLabel("Error downloading MiSTer.ini");
                    System.Threading.Thread.Sleep(intErrorPause);
                    return;
                };
                this.cmbMiSTerDir_TextChanged(null, null);
                this.cmbMiSTerDir.TextChanged += new System.EventHandler(this.cmbMiSTerDir_TextChanged);


                treeViewExtras.ExpandAll();
                foreach (System.Windows.Forms.TreeNode objNode in treeViewExtras.Nodes)
                {
                    objNode.Checked = true;
                };                
                treeViewCores.ExpandAll();
                loadCoresTree();

                foreach (System.Windows.Forms.TreeNode objNode in treeViewCores.Nodes)
                {
                    objNode.Checked = true;
                };

                btn1ClickSetup.Enabled = true;
                btnAdvanced.Enabled = true;
                writeStatusLabel_TS("Ready");

                blnInitialized = true;
            };
        }

        private void treeViewCores_AfterCheck(object sender, TreeViewEventArgs e)
        {
            switch (e.Node.Name) {
                case "root":
                case "cores":
                case "console-cores":
                case "arcade-cores":
                case "service-cores":
                    foreach (System.Windows.Forms.TreeNode objNode in e.Node.Nodes)
                        objNode.Checked = e.Node.Checked;
                    break;
            };
        }

        private void btnDownloadCores_Click(object sender, EventArgs e)
        {
            if (btnDownloadCores.Text == "Download")
            {
                enableCoresUI(false);
                createSDInstallerSemaphore(getControlText_TS(cmbMiSTerDir), System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
                writeStatusLabel("Downloading cores");
                blnDownloadingCores = true;
                asyncDownloadCores(treeViewCores.Nodes, cmbMiSTerDir.Text, System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
            }
            else {
                blnDownloadingCores = false;
            };
        }

        private void linkLabelMiSTerWiki_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(strMiSTerURL + "/wiki");
        }

        private void treeViewCores_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag!=null && e.Node.Tag.ToString()!="") {
                System.Diagnostics.Process.Start(e.Node.Tag.ToString());
            };
        }

        private void hideTabHeaders() {
            tabControlSections.Appearance = TabAppearance.FlatButtons;
            tabControlSections.ItemSize = new Size(0, 1);
            tabControlSections.SizeMode = TabSizeMode.Fixed;
        }

        System.Windows.Forms.TabAppearance tabControlSectionsOriginalAppearance;
        Size tabControlSectionsOriginalSize;
        System.Windows.Forms.TabSizeMode tabControlSectionsSizeMode;
        bool blnAdvancedMode=true;
        private void enableAdvancedMode(bool enable) {
            if (enable) {
                if (!blnAdvancedMode)
                {

                    tabControlSections.Appearance = tabControlSectionsOriginalAppearance;
                    tabControlSections.ItemSize = tabControlSectionsOriginalSize;
                    tabControlSections.SizeMode = tabControlSectionsSizeMode;
                    blnAdvancedMode = true;
                };
            }
            else {
                if (blnAdvancedMode)
                {
                    tabControlSectionsOriginalAppearance = tabControlSections.Appearance;
                    tabControlSectionsOriginalSize = tabControlSections.ItemSize;
                    tabControlSectionsSizeMode = tabControlSections.SizeMode;
                    tabControlSections.Appearance = TabAppearance.FlatButtons;
                    tabControlSections.ItemSize = new Size(0, 1);
                    tabControlSections.SizeMode = TabSizeMode.Fixed;
                    blnAdvancedMode = false;
                }
            };
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            initializeConfigurator();
        }

        private void btnDownloadExtras_Click(object sender, EventArgs e)
        {
            if (btnDownloadExtras.Text == "Download")
            {
                enableExtrasUI(false);
                createSDInstallerSemaphore(getControlText_TS(cmbMiSTerDir), System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
                writeStatusLabel("Downloading Extras");
                blnDownloadingExtras = true;
                asyncDownloadExtras(treeViewExtras.Nodes, cmbMiSTerDir.Text);
            }
            else
            {
                blnDownloadingExtras = false;
            };
        }

        private void treeViewExtras_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag.ToString() != "")
            {
                System.Diagnostics.Process.Start(e.Node.Tag.ToString().Split('|')[0]);
            };
        }

        private void createSDInstallerSemaphore(String baseDirectory, String workDirectory)
        {
            String semaphoreFile = null;
            if (System.IO.File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(baseDirectory, "linux"), "linux.img")))
            {
                semaphoreFile = System.IO.Path.Combine(workDirectory, "release_" + System.IO.File.GetLastWriteTimeUtc(System.IO.Path.Combine(System.IO.Path.Combine(baseDirectory, "linux"), "linux.img")).ToString("yyyyMMdd") + ".rar");
            }
            else {
                semaphoreFile = System.IO.Path.Combine(workDirectory, "release_" + System.DateTime.Today.ToString("yyyyMMdd") + ".rar");            
            };
            if (!System.IO.File.Exists(semaphoreFile)) {
                if (!System.IO.Directory.Exists(workDirectory) && !CreateDirectorySafe(workDirectory)) return;
                createEmptyFile(semaphoreFile);
            };
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            enableAdvancedMode(!blnAdvancedMode);
        }

        private void chkEnableSamba_CheckedChanged(object sender, EventArgs e)
        {
            pnlSamba.Enabled = chkEnableSamba.Checked;
        }

        private void chkEnableWiFi_CheckedChanged(object sender, EventArgs e)
        {
            pnlWiFi.Enabled = chkEnableWiFi.Checked;
        }

        private void btnCompatibilityPreset_Click(object sender, EventArgs e)
        {
            cmbVideoMode.SelectedIndex = 0;
            chkEnablePAL_NTSC.Checked = false;
            // cmbScalingMode.SelectedIndex = 0;
            cmbVSyncMode.SelectedIndex = 0;
        }

        private void btnOptimalPreset_Click(object sender, EventArgs e)
        {
            cmbVideoMode.SelectedIndex = 8;
            chkEnablePAL_NTSC.Checked = true;
            // cmbScalingMode.SelectedIndex = 0;
            cmbVSyncMode.SelectedIndex = 2;
        }

        private void btn1ClickSetup_Click(object sender, EventArgs e)
        {
            if (btn1ClickSetup.Text != "Stop")
            {
                writeStatusLabel_TS("Saving Configuration");
                saveMiSTerINI(ref strMiSTerINI_Current, System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "MiSTer.ini"));
                saveSamba();
                saveWiFi();
                enableWizardUI(false);
                enableCoresUI(false);
                enableExtrasUI(false);
                createSDInstallerSemaphore(getControlText_TS(cmbMiSTerDir), System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
                writeStatusLabel("Downloading cores");
                blnDownloadingCores = true;
                blnDownloadingExtras = true;
                blnRunningWizard = true;
                asyncDownloadCores(treeViewCores.Nodes, cmbMiSTerDir.Text, System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), strWorkDir));
            }
            else {
                blnDownloadingCores = false;
                blnDownloadingExtras = false;
            };
        }

        private void cmbVideoMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbVideoMode.SelectedIndex)
            {
                case 0:
                case 7:
                case 8:
                case 9:
                    chkEnablePAL_NTSC.Enabled = true;
                    break;
                default:
                    chkEnablePAL_NTSC.Checked = false;
                    chkEnablePAL_NTSC.Enabled = false;
                    break;
            }
        }

        private void linkLabelUnixInstaller_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            String strRepositoryURL = linkLabelUnixInstaller.Tag.ToString().Split('|')[0];
            String strFile = linkLabelUnixInstaller.Tag.ToString().Split('|')[1];
            System.Diagnostics.Process.Start(strRepositoryURL);
            downoadExtra(getControlText_TS(linkLabelUnixInstaller), strRepositoryURL, strFile, getControlText_TS(cmbMiSTerDir));
            writeStatusLabel("Ready");
        }

        private void cmbMiSTerDir_TextChanged(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "MiSTer.ini")))
            {
                strMiSTerINI_Current = readFileSTR(System.IO.Path.Combine(getControlText_TS(cmbMiSTerDir), "MiSTer.ini"));
            }
            else
            {
                strMiSTerINI_Current = strMiSTerINI_GitHub;
            };
            readMiSTerINI(strMiSTerINI_Current);
        }

        private void ConfiguratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btn1ClickSetup.Text == "Stop") btn1ClickSetup_Click(null, null);
            if (btnDownloadCores.Text == "Stop") btnDownloadCores_Click(null, null);
            if (btnDownloadExtras.Text == "Stop") btnDownloadExtras_Click(null, null);
        }

    }
}
