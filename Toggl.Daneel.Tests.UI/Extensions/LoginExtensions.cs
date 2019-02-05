using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class LoginExtensions
    {
        public static void WaitForLoginScreen(this IApp app)
        {
            app.SkipToLoginPage();
        }

        public static void CheckThatLoginButtonIsDisabled(this IApp app)
        {
            // We don't disable the login button on iOS, so we just check that 
            // the user doesn't get logges in.
            app.Tap(Login.LoginButton);
            app.WaitForNoElement(Main.StartTimeEntryButton);
        }
    }
}
