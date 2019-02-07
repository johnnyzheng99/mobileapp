using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Views.Client;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : BaseTableViewSource<string, SelectableClientBaseViewModel>
    {
        private const int rowHeight = 48;

        public ClientTableViewSource(UITableView tableView)
        {
            tableView.RowHeight = rowHeight;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var model = ModelAt(indexPath);
            var identifier = model is SelectableClientCreationViewModel ? CreateClientViewCell.Identifier : ClientViewCell.Identifier;
            var cell = (BaseTableViewCell<SelectableClientBaseViewModel>)tableView.DequeueReusableCell(identifier);
            cell.Item = model;
            return cell;
        }
    }
}
