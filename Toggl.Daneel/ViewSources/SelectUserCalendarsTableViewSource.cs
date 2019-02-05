using System.Collections.Generic;
using System.Linq;
using Foundation;
using Toggl.Daneel.Cells.Calendar;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class SelectUserCalendarsTableViewSource : ReloadTableViewSource<string, SelectableUserCalendarViewModel>
    {
        private const int rowHeight = 48;
        private const int headerHeight = 48;

        public UIColor SectionHeaderBackgroundColor { get; set; } = UIColor.White;

        public SelectUserCalendarsTableViewSource(UITableView tableView) : base(
            SelectableUserCalendarViewCell.CellConfiguration(SelectableUserCalendarViewCell.Identifier)
        )
        {
            ConfigureHeader = viewForHeaderInSection;

            tableView.RegisterNibForCellReuse(SelectableUserCalendarViewCell.Nib, SelectableUserCalendarViewCell.Identifier);
            tableView.RegisterNibForHeaderFooterViewReuse(UserCalendarListHeaderViewCell.Nib, UserCalendarListHeaderViewCell.Identifier);
            tableView.SectionHeaderHeight = headerHeight;
            tableView.RowHeight = rowHeight;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);

            var cell = (SelectableUserCalendarViewCell)tableView.CellAt(indexPath);
            cell.ToggleSwitch();

            tableView.DeselectRow(indexPath, true);
        }

        private UIView viewForHeaderInSection(UITableView tableView, int section, string sourceName)
        {
            var header = tableView.DequeueReusableHeaderFooterView(UserCalendarListHeaderViewCell.Identifier) as UserCalendarListHeaderViewCell;
            header.Item = sourceName;
            header.ContentView.BackgroundColor = SectionHeaderBackgroundColor;
            return header;
        }
    }
}
