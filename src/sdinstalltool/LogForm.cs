using System;
using System.Windows.Forms;

namespace SDInstallTool
{
    public partial class LogForm : Form
    {
        #region Properties

        #endregion

        #region Constructors
        public LogForm()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Methods

        public void logMessageLine(String message)
        {
            #region Ensure executed in main thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    logMessageLine(message);
                });

                return;
            }
            #endregion Ensure executed in main thread

            if (txtLogMessages != null && !txtLogMessages.IsDisposed)
            {
                txtLogMessages.AppendText(message);
                txtLogMessages.AppendText(Environment.NewLine);
            }
        }

        public void logMessage(String message)
        {
            #region Ensure executed in main thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    logMessage(message);
                });

                return;
            }
            #endregion Ensure executed in main thread

            txtLogMessages.AppendText(message);
        }

        #endregion

        #region Event Handlers
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLogMessages.Clear();
        }

        private void buttonClip_Click(object sender, EventArgs e)
        {
            var text = txtLogMessages.Text;
            Clipboard.SetText(text);
        }

        #endregion Event Handlers
    }
}
