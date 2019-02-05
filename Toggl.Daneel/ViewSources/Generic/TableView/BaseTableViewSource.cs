using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CoreGraphics;
using Foundation;
using Toggl.Foundation.MvvmCross.Collections;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public delegate UITableViewCell CellConfiguration<TModel>(UITableView tableView, NSIndexPath indexPath, TModel model);
    public delegate UIView HeaderConfiguration<THeader>(UITableView tableView, int section, THeader header);

    public abstract class BaseTableViewSource<THeader, TModel> : UITableViewSource
    {
        private readonly CellConfiguration<TModel> configureCell;

        protected IImmutableList<CollectionSection<THeader, TModel>> Sections { get; set; }

        public EventHandler<TModel> OnItemTapped { get; set; }
        public EventHandler<CGPoint> OnScrolled { get; set; }

        public HeaderConfiguration<THeader> ConfigureHeader { get; set; }

        public BaseTableViewSource(CellConfiguration<TModel> configureCell, IEnumerable<CollectionSection<THeader, TModel>> sections)
        {
            Sections = sections.ToImmutableList();
            this.configureCell = configureCell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);
            OnItemTapped?.Invoke(this, Sections[indexPath.Section].Items[indexPath.Row]);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            OnScrolled?.Invoke(this, scrollView.ContentOffset);
        }

        public override nint NumberOfSections(UITableView tableView)
            => Sections.Count;

        public override nint RowsInSection(UITableView tableview, nint section)
            => Sections[(int)section].Items.Count;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            => configureCell(tableView, indexPath, Sections[indexPath.Section].Items[indexPath.Row]);

        public override UIView GetViewForHeader(UITableView tableView, nint section)
            => ConfigureHeader == null ? null : ConfigureHeader(tableView, (int)section, Sections[(int)section].Header);

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            => ConfigureHeader == null ? 0 : tableView.SectionHeaderHeight;
    }
}
