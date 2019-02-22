using Toggl.Foundation.MvvmCross.Interfaces;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels.Selectable
{
    public sealed class SelectableUserCalendarViewModel : IDiffable<SelectableUserCalendarViewModel>
    {
        public string Id { get; }

        public string Name { get; }

        public string SourceName { get; }

        public bool InitiallySelected { get; }

        public long Identifier => Id.GetHashCode();

        public SelectableUserCalendarViewModel(UserCalendar calendar, bool initiallySelected)
        {
            Ensure.Argument.IsNotNull(calendar, nameof(calendar));

            Id = calendar.Id;
            Name = calendar.Name;
            SourceName = calendar.SourceName;
            InitiallySelected = initiallySelected;
        }

        public bool Equals(SelectableUserCalendarViewModel other)
        {
            return Id == other.Id;
        }
    }
}
