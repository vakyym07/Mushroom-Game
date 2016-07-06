using System;
using System.Windows.Forms;

namespace MyGame
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.Run(new MyForm());
        }
    }
}
