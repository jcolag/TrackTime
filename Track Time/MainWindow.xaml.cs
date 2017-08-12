namespace Track_Time
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private readonly double epsilon = 0.01;
        private string lastWindow = string.Empty;
        private DateTime since = new DateTime();
        private uint interval = 5;

        private uint minIdle = 300;

        private DispatcherTimer clock = null;

        private bool isDirty = false;

        private uint logLength = 0;

        private WinEventDelegate windowChanged = null;

        public MainWindow()
        {
            InitializeComponent();
            this.windowChanged = new WinEventDelegate(WindowFocusChanged);
            IntPtr m_hhook = SetWinEventHook(
                EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero,
                this.windowChanged,
                0,
                0,
                WINEVENT_OUTOFCONTEXT);
            this.clock = new DispatcherTimer();
            this.clock.Interval = new TimeSpan(0, 0, (int)this.interval);
            this.clock.Tick += clock_Tick;
            this.clock.Start();
            this.since = DateTime.Now;
            textInterval.Text = this.interval.ToString();
        }

        void clock_Tick(object sender, EventArgs e)
        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("Kernel32.dll")]
        private static extern UInt32 GetTickCount();

        {
            String title = FindCurrentWindow();
            if (title == this.lastWindow)
            {
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan duration = now - this.since;
            String log = now.ToString("u") + "," + this.lastWindow + "," + duration.ToString() + "\n";
            bool isAtBottom = false;

            if (Math.Abs(TextScroll.VerticalOffset - TextScroll.ScrollableHeight) < this.epsilon)
            {
                isAtBottom = true;
            }

            WindowLog.Text += log;
            this.logLength += 1;
            textLogLength.Text = this.logLength.ToString();
            this.since = now;
            this.lastWindow = title;

            uint idle = GetIdleTime();
            if (idle > this.minIdle)
            {
                TimeSpan idleTime = new TimeSpan(0, 0, (int)idle);
                log = "Also idle for approximately " + idleTime.ToString() + "\n";
            }

            if (isAtBottom)
            {
                TextScroll.ScrollToEnd();
            }

            this.isDirty = true;
        }

        private String FindCurrentWindow()
        {
            const int count = 512;
            var text = new StringBuilder(count);
            IntPtr handle = GetForegroundWindow();
            String title = String.Empty;
            if (GetWindowText(handle, text, count) > 0)
            {
                title = text.ToString();
            }

            return title;
        }

        public void WindowFocusChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            clock_Tick(null, new EventArgs());
        }

        private uint GetIdleTime()
        {
            /*
             * Since we're working with old-time "ticks," which are (maybe)
             * some number of milliseconds that depend on system load,
             * this is only very approximate.
             */
            uint now = GetTickCount();
            uint idlestart = now;
            uint idleticks = 0;
            LASTINPUTINFO lastinput = new LASTINPUTINFO();
            lastinput.cbSize = (uint)Marshal.SizeOf(lastinput);
            if (GetLastInputInfo(ref lastinput))
            {
                idlestart = lastinput.dwTime;
            }

            idleticks = (now - idlestart) / 100;
            return idleticks;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.clock.IsEnabled)
            {
                clock_Tick(sender, new EventArgs());
                this.clock.Stop();
                this.lastWindow = "* Paused *";
                System.Console.WriteLine("Pause");
            }
            else
            {
                this.clock.Start();
                System.Console.WriteLine("Unpause");
            }
        }

        private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void textInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null)
            {
                return;
            }

            String possibleInterval = box.Text;
            Int16 newInterval = Int16.Parse(possibleInterval);
            if (newInterval > 0)
            {
                this.clock.Interval = new TimeSpan(0, 0, newInterval);
                this.interval = (uint)newInterval;
            }
        }

        private void Clear_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WindowLog.Clear();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
			DateTime now = DateTime.Now;
			String today = String.Format("{0}-{1}-{2}.csv", now.Year, now.Month, now.Day);
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.FileName = today;
            sfd.ShowDialog();
            String filename = sfd.FileName;
            if (String.IsNullOrWhiteSpace(filename))
            {
                return;
            }

            System.IO.StreamWriter outfile = System.IO.File.CreateText(filename);
            String contents = WindowLog.Text;
            outfile.Write(contents.Replace("\n", outfile.NewLine));
            outfile.Flush();
            outfile.Close();
            this.isDirty = false;
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.isDirty)
            {
                MessageBoxResult answer = MessageBox.Show("There are unsaved changes.  Exit anyway?",
                    "Exiting...", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (answer == MessageBoxResult.No)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
    }
}