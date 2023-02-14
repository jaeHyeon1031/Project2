using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _49_NetworkStreamMultiThreadFormClient
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new Form1());
            Form1 login = new Form1(); 
            if(login.ShowDialog() == DialogResult.OK)
            {
                Form2 form2 = new Form2();
                Application.Run(form2);
            }
        }
    }
}
