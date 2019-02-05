using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class StartTimeEntryExtensions
    {
        public static void WaitForStartTimeEntryScreen(this IApp app)
        {
            app.WaitForMainScreen();

            app.Tap(Main.StartTimeEntryButton);
            app.WaitForElement(StartTimeEntry.DoneButton);
        }

        public static void AddTagInStartView(this IApp app, string tag, bool shouldCreateTag = true)
        { 
            app.EnterText($"#{tag}");

            if (shouldCreateTag)
            {
                app.TapCreateTag(tag);
                return;
            }

            app.TapSelectTag(tag);
        }

        public static void CreateProjectInStartView(this IApp app, string projectName, string clientName = null)
        {
            app.EnterText($"@{projectName}");
            app.TapCreateProject(projectName);

            if (!string.IsNullOrEmpty(clientName))
            {
                app.Tap(NewProject.ChangeClient);
                app.EnterText(clientName);
                app.TapCreateClient(clientName);
            }

            app.WaitForElement(NewProject.CreateButton);
            app.Tap(NewProject.CreateButton);
        }
    }
}
