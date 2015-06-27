using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace mairo
{
    class Program
    {
        static NeuroEngine ne;
        static LevelEngine le;

        const int SWP_NOZORDER = 0x4;
        const int SWP_NOACTIVATE = 0x10;

        [DllImport("kernel32")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, int flags);

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Init...");
            le = new LevelEngine();
            int delay = 23;
            Display disp = new Display();
            disp.le = le;
            disp.Show();
            disp.BringToFront();
            le.disp = disp;
            SetWindowPosition(0, 0, 668, 331);
            disp.Left = 0;
            disp.Top = 331;
            while (true)
            {
                Thread.Sleep(delay);
                Application.DoEvents();
                if (le.Pause)
                    continue;
                le.Draw();
                disp.Refresh();
                le.Advance();
                if(Console.KeyAvailable)
                    switch (Console.ReadKey().KeyChar)
                    {
                        case 'w':
                            if (delay > 10)
                                delay -= 10;
                            break;
                        case 's':
                            if (delay < 500)
                                delay += 10;
                            break;
                        case (char)13:
                            le.ResetLevel();
                            break;
                        case (char)27:
                            return;

                    }
                Console.WriteLine("Delay : {0}ms", delay);
            }
        }

        public static void SetWindowPosition(int x, int y, int width, int height)
        {
            SetWindowPos(Handle, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        public static IntPtr Handle
        {
            get
            {
                //Initialize();
                return GetConsoleWindow();
            }
        }
    }
}
