using System.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Data;
using System.ServiceModel.Web;
using System.ServiceModel;

namespace Track_My_Work
{
    public partial class Form : System.Windows.Forms.Form
    {
        private const string site = "site.html";
        private static string path = AppDomain.CurrentDomain.BaseDirectory + site;
        private NotifyIcon notifyIcon;
        public DataTable Source { get; set; }
        public Form()
        {
            InitializeComponent();

            DBProvider.CreateDB();
            dataGridView.DataSource = Source;

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(Tracker.SessionSwitch);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(LoadGridSource);

            Tracker.SessionSwitch(null, new SessionSwitchEventArgs(SessionSwitchReason.SessionLogon));
            LoadGridSource(null, null);

            SetViewSettings();

            var host = new WebServiceHost(typeof(Service), new Uri("http://localhost:8000/"));
            var ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
            host.Open();

           var s = StatProvider. GetStatisticsInfo();
        }

        private void SetViewSettings(object sender=null, EventArgs e=null)
        {
            notifyIcon = new NotifyIcon {Text = "Track My Work", Visible = true, Icon = new Icon("Main.ico")};
            notifyIcon.Click += new EventHandler(this.notifyIcon_Click);

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
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
            var filePath = string.Format("file:///{0}", path);
            var uri = new System.Uri(filePath);
            var converted = uri.AbsoluteUri;

            System.Diagnostics.Process.Start(defBrowser, converted);
            this.Activate();
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
