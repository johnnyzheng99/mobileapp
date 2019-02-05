using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class LoginExtensions
    {
        public static void WaitForLoginScreen(this IApp app)
        {
            //Giskard doesn't have the onboarding screen
        }

        public static void CheckThatLoginButtonIsDisabled(this IApp app)
        {
            var button = app.Query(Login.LoginButton).First();
            var isButtonDisabled = !button.Enabled;
            Assert.True(isButtonDisabled);
        }
    }
}
