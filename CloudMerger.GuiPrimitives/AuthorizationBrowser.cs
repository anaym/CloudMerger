using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudMerger.GuiPrimitives
{
    public static class AuthorizationBrowser
    {

        public static Thread ShowNew(Action<WebBrowser, Form> configurator)
        {
            return StaApplication.StartNew(() => StaConfigurator(configurator));
        }

        public delegate bool PagePredicate(Uri uri, string content);
        public static async Task<Tuple<Uri, string>> CreateNewAsync(Uri startPage, PagePredicate finalPage)
        {
            Tuple<Uri, string> result = null;
            Action<WebBrowser, Form> configurator = (b, f) =>
            {
                b.Navigated += (sender, args) =>
                {
                    if (finalPage(args.Url, b.DocumentText))
                    {
                        result = Tuple.Create(args.Url, b.DocumentText);
                        f.Close();
                    }
                };
                b.Navigate(startPage);
            };
            await CreateNewAsync(configurator);
            return result;
        }

        public static async Task CreateNewAsync(Action<WebBrowser, Form> configurator)
        {
            var thread = await Task.Run(() => ShowNew(configurator));
            thread.Join();
        }

        private static Form StaConfigurator(object configurator)
        {
            var form = new AuthorizationBrowserForm();
            ((Action<WebBrowser, Form>) configurator)(form.Browser, form);
            return form;
        }
    }

    public class AuthorizationBrowserForm : Form
    {
        public readonly WebBrowser Browser;
        public object Result { get; }

        public AuthorizationBrowserForm()
        {
            NativeMethods.SuppressCookiePersist();

            ClientSize = new Size(1080, 720);

            var table = new TableLayoutPanel {Dock = DockStyle.Fill};
            table.RowStyles.Add(new RowStyle {SizeType = SizeType.Percent, Height = 1f});
            Controls.Add(table);

            Browser = new WebBrowser {Dock = DockStyle.Fill};
            table.Controls.Add(Browser, 0, 0);

            var progressBar = new ProgressBar {Dock = DockStyle.Fill};
            table.Controls.Add(progressBar, 0, 1);

            Browser.ProgressChanged += (sender, args) =>
            {
                if (args.MaximumProgress < 0 || args.CurrentProgress < 0)
                {
                    progressBar.Maximum = 100;
                    progressBar.Value = 100;
                }
                else
                {
                    progressBar.Maximum = (int) args.MaximumProgress;
                    progressBar.Value = (int) args.CurrentProgress;
                }
            };
            Text = $"Authorization browser";
        }


        public Thread StartApplicationThread()
        {
            var thread = CreateApplicationThread();
            thread.Start();
            return thread;
        }

        public Thread CreateApplicationThread()
        {
            var appThread = new Thread(ApplicationRunner);
            appThread.SetApartmentState(ApartmentState.STA);
            return appThread;
        }

        private void ApplicationRunner()
        {
            Application.Run(this);
        }
    }

    public static class NativeMethods
    {
        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption,
                                                     IntPtr lpBuffer, int lpdwBufferLength);

        public static void SuppressCookiePersist()
        {
            int dwOption = 81; //INTERNET_OPTION_SUPPRESS_BEHAVIOR
            int option = 3; // INTERNET_SUPPRESS_COOKIE_PERSIST

            IntPtr optionPtr = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(optionPtr, option);

            InternetSetOption(IntPtr.Zero, dwOption, optionPtr, sizeof(int));
            Marshal.FreeHGlobal(optionPtr);
        }
    }
}