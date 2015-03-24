using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(100);
            Process.Start(@"update.exe", "-l -y -sd1");
        }
    }
}
