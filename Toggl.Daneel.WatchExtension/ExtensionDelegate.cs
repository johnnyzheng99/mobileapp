﻿using Foundation;
using Toggl.Daneel.WatchExtension.Extensions;
using WatchConnectivity;
using WatchKit;

namespace Toggl.Daneel.WatchExtension
{
    [Register("ExtensionDelegate")]
    public class ExtensionDelegate : WKExtensionDelegate
    {
        public override void ApplicationDidFinishLaunching()
        {
            // Perform any final initialization of your application.
            if (WCSession.IsSupported)
            {
                WCSession.DefaultSession.Delegate = new WatchSessionHandler();
                WCSession.DefaultSession.ActivateSession();
            }
        }

        public override void ApplicationDidBecomeActive()
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive.
            // If the application was previously in the background, optionally refresh the user interface.

            if (!WCSession.DefaultSession.ReceivedApplicationContext.ContainsKey("LoggedIn".ToNSString()))
            {
                WKExtension.SharedExtension.InvokeOnMainThread(() =>
                {
                    WKInterfaceController.ReloadRootControllers(new[] { "LoginInterfaceController" }, null);
                });
                return;
            }
        }

        public override void ApplicationWillResignActive()
        {
            // Sent when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions
            // (such as an incoming phone call or SMS message) or when the user quits the application
            // and it begins the transition to the background state.
            // Use this method to pause ongoing tasks, disable timers, etc.
        }
    }
}