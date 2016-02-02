using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace Track_My_Work
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form());

            DBProvider.CreateDB();
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(Tracker.SessionSwitch);
        }
    }
}
