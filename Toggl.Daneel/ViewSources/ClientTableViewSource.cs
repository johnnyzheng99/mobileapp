using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Views.Client;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ReloadTableViewSource<string, SelectableClientBaseViewModel>
    {
        private const int rowHeight = 48;

        public ClientTableViewSource(UITableView tableView) : base(configureCell)
        {
            tableView.RowHeight = rowHeight;
        }

        private static UITableViewCell configureCell<SelectableClientBaseViewModel>(UITableView tableView, NSIndexPath indexPath, SelectableClientBaseViewModel model)
        {
            var identifier = model is SelectableClientCreationViewModel ? CreateClientViewCell.Identifier : ClientViewCell.Identifier;
            var cell = (BaseTableViewCell<SelectableClientBaseViewModel>)tableView.DequeueReusableCell(identifier);
            cell.Item = model;
            return cell;
        }
    }
}
