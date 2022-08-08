using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace INO3D
{
    public partial class MainWindow : Window
    {
        #region Static

        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);

        #endregion

        #region Fields

        private readonly Process unityProcess;
        private IntPtr unityHwnd;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                unityProcess = new Process();
                unityProcess.StartInfo.FileName =
                    @"E:\Projetos pessoais\INO3D\INO3D Viewer\bin\INO3D Viewer.exe";
                unityProcess.StartInfo.Arguments =
                    "-parentHWND " + UnityContainer.Handle.ToInt32() + " " + Environment.CommandLine;

                unityProcess.StartInfo.UseShellExecute = true;
                unityProcess.StartInfo.CreateNoWindow = true;

                unityProcess.Start();
                unityProcess.WaitForInputIdle();

                EnumChildWindows(UnityContainer.Handle, WindowEnum, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHwnd = hwnd;
            SendMessage(unityHwnd, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
            return 0;
        }

        private void UnityContainer_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveWindow(unityHwnd, 0, 0, (int)e.NewSize.Width, (int)e.NewSize.Height, true);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            try
            {
                SendMessage(unityHwnd, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
                unityProcess.CloseMainWindow();
                unityProcess.Close();

                unityProcess.WaitForExit();
                while (!unityProcess.HasExited)
                    unityProcess.Kill();
            }
            catch (Exception)
            {
                //
            }
        }

        #endregion
    }
}