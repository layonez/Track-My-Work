using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Track_My_Work
{
    public partial class Form : System.Windows.Forms.Form
    {
        private const string FileName = "fileName.html";
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + FileName;
        private NotifyIcon _notifyIcon;
        public DataTable Source { get; set; }
        public Form()
        {
            try
            {
                InitializeComponent();

                DBProvider.CreateDBIfNotExist();
                dataGridView.DataSource = Source;

                SystemEvents.SessionSwitch += Tracker.SessionSwitch;
                SystemEvents.SessionSwitch += LoadGridSource;

                Tracker.SessionSwitch(null, new SessionSwitchEventArgs(SessionSwitchReason.SessionLogon));
                LoadGridSource(null, null);

                SetViewSettings();

                var host = new WebServiceHost(typeof(Service), new Uri("http://localhost:8001/"));
                var ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
                host.Open();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void SetViewSettings(object sender=null, EventArgs e=null)
        {
            _notifyIcon = new NotifyIcon {Text = "Track My Work", Visible = true, Icon = new Icon("Main.ico")};
            _notifyIcon.Click += this.notifyIcon_Click;

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            this.Visible = false;
            Opacity = 0;
        }

        private void LoadGridSource(object sender, SessionSwitchEventArgs e)
        {
            var dt = DBProvider.ReadAll();
            
            Source = dt;
            dataGridView.DataSource = Source;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            var defBrowser = GetDefaultBrowserPath();
            var filePath = string.Format("file:///{0}", Path);
            var uri = new Uri(filePath);
            var converted = uri.AbsoluteUri;

            Process.Start(defBrowser, converted);
        }
        private static string GetDefaultBrowserPath()
        {
            string key = @"HTTP\shell\open\command";
            using (RegistryKey registrykey = Registry.ClassesRoot.OpenSubKey(key, false))
            {
                return ((string)registrykey.GetValue(null, null)).Split('"')[1];
            }
        }

    }
}
