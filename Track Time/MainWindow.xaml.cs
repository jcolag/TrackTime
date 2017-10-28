//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Colagioia Industries">
//     Copyright (c) John Colagioia, available under the GPLv3.
// </copyright>
//-----------------------------------------------------------------------
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
        /// <summary>
        /// Out of context window events.
        /// </summary>
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        /// <summary>
        /// The foreground-window-changed event.
        /// </summary>
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        /// <summary>
        /// The epsilon value for height comparisons.
        /// </summary>
        private readonly double epsilon = 0.01;

        /// <summary>
        /// The last foreground window detected.
        /// </summary>
        private string lastWindow = string.Empty;

        /// <summary>
        /// The time since the last event.
        /// </summary>
        private DateTime since = new DateTime();

        /// <summary>
        /// The title-checking interval.
        /// </summary>
        private uint interval = 5;

        /// <summary>
        /// The minimum idle duration in "ticks."
        /// </summary>
        private uint minIdle = 300;

        /// <summary>
        /// The application clock.
        /// </summary>
        private DispatcherTimer clock = null;

        /// <summary>
        /// The flag tracking whether the log has been saved.
        /// </summary>
        private bool isDirty = false;

        /// <summary>
        /// The length of the log.
        /// </summary>
        private uint logLength = 0;

        /// <summary>
        /// The window changed delegate.
        /// </summary>
        private WinEventDelegate windowChanged = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Track_Time.MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.windowChanged = new WinEventDelegate(this.WindowFocusChanged);
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
            this.clock.Tick += this.clock_Tick;
            this.clock.Start();
            this.since = DateTime.Now;
            textInterval.Text = this.interval.ToString();
        }

        /// <summary>
        /// Window event delegate.
        /// </summary>
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

        /// <summary>
        /// Event handler for clock ticks and related work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments</param>
        private void clock_Tick(object sender, EventArgs e)
        {
            string title = this.FindCurrentWindow();
            if (title == this.lastWindow)
            {
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan duration = now - this.since;
            string log = now.ToString("u") + "," + this.lastWindow + "," + duration.ToString() + "\n";
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

            uint idle = this.GetIdleTime();
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

        /// <summary>
        /// Finds the current window title.
        /// </summary>
        /// <returns>The current window title.</returns>
        private string FindCurrentWindow()
        {
            const int Count = 512;
            var text = new StringBuilder(Count);
            IntPtr handle = GetForegroundWindow();
            string title = string.Empty;
            if (GetWindowText(handle, text, Count) > 0)
            {
                title = text.ToString();
            }

            return title;
        }

        /// <summary>
        /// Handler for window-focus changes.
        /// </summary>
        /// <param name="hWinEventHook">The window event hook.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="idObject">The object's identifier.</param>
        /// <param name="idChild">The identifier of the child object.</param>
        /// <param name="dwEventThread">The event thread's identifier.</param>
        /// <param name="dwmsEventTime">The event time (in milliseconds).</param>
        public void WindowFocusChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            this.clock_Tick(null, new EventArgs());
        }

        /// <summary>
        /// Gets the idle time.
        /// </summary>
        /// <returns>The idle time (in "ticks").</returns>
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

        /// <summary>
        /// Pauses the logging.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.clock.IsEnabled)
            {
                this.clock_Tick(sender, new EventArgs());
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

        /// <summary>
        /// Handler to ensure methods are available.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Handler for when the interval text changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void textInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null)
            {
                return;
            }

            string possibleInterval = box.Text;
            Int16 newInterval = Int16.Parse(possibleInterval);
            if (newInterval > 0)
            {
                this.clock.Interval = new TimeSpan(0, 0, newInterval);
                this.interval = (uint)newInterval;
            }
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Clear_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WindowLog.Clear();
        }

        /// <summary>
        /// Saves the log.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            string today = string.Format("Time-{0:D4}-{1:D2}-{2:D2}.csv", now.Year, now.Month, now.Day);
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = today;
            sfd.ShowDialog();
            string filename = sfd.FileName;
            if (string.IsNullOrWhiteSpace(filename))
            {
                return;
            }

            System.IO.StreamWriter outfile = System.IO.File.CreateText(filename);
            string contents = WindowLog.Text;
            outfile.Write(contents.Replace("\n", outfile.NewLine));
            outfile.Flush();
            outfile.Close();
            this.isDirty = false;
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.isDirty)
            {
                MessageBoxResult answer = MessageBox.Show(
                    "There are unsaved changes.  Exit anyway?",
                    "Exiting...",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (answer == MessageBoxResult.No)
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Time of the last input event, as defined in User32.dll.
        /// </summary>
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
    }
}