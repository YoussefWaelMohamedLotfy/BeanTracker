using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading;

namespace BeanTracker.MAUI.WinUI;

public class CustomProgram
{
    [STAThread]
    static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        
        bool isRedirect = false;

        // Try to register as the main instance. If it fails, it means another instance is running.
        var keyInstance = AppInstance.FindOrRegisterForKey("beantracker");
        
        if (!keyInstance.IsCurrent)
        {
            // Another instance is already running. Redirect the activation to it.
            var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedEventArgs != null)
            {
                keyInstance.RedirectActivationToAsync(activatedEventArgs).AsTask().Wait();
            }
            isRedirect = true;
        }

        if (!isRedirect)
        {
            // We are the main instance. Start the application normally.
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
    }
}
