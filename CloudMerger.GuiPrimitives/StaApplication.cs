using System;
using System.Threading;
using System.Windows.Forms;

namespace CloudMerger.GuiPrimitives
{
    public static class StaApplication
    {
        public static Thread StartNew(Func<Form> configurator)
        {
            var appThread = new Thread(ApplicationRunner);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start(configurator);
            return appThread;
        }
        
        private static void ApplicationRunner(object configurator)
        {
            var form = ((Func<Form>) configurator)();
            Application.Run(form);
        }
    }
}