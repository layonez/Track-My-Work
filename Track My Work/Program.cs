using System;
using System.Windows.Forms;
using Microsoft.Win32;

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

            DBProvider.CreateDBIfNotExist();
            SystemEvents.SessionSwitch += Tracker.SessionSwitch;
        }
    }
}
