using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels.Selectable
{
    public sealed class SelectableUserCalendarViewModel
    {
        public string Id { get; }

        public string Name { get; }

        public string SourceName { get; }

        public bool InitiallySelected { get; }

        public SelectableUserCalendarViewModel(UserCalendar calendar, bool initiallySelected)
        {
            Ensure.Argument.IsNotNull(calendar, nameof(calendar));

            Id = calendar.Id;
            Name = calendar.Name;
            SourceName = calendar.SourceName;
            InitiallySelected = initiallySelected;
        }
    }
}
