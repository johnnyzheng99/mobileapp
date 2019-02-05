using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using Toggl.Multivac.Extensions;

namespace Toggl.Daneel.Views
{
    public partial class TimeEntriesLogHeaderView : UITableViewHeaderFooterView
    {
        public static readonly string Identifier = "timeEntryLogHeaderCell";

        public static readonly NSString Key = new NSString(nameof(TimeEntriesLogHeaderView));
        public static readonly UINib Nib;

        private IReadOnlyList<TimeEntryViewModel> items;
        public IReadOnlyList<TimeEntryViewModel> Items
        {
            set
            {
                items = value;
                updateView();
            }
        }

        private DateTimeOffset now;
        public DateTimeOffset Now
        {
            set
            {
                now = value;
                updateView();
            }
        }

        static TimeEntriesLogHeaderView()
        {
            Nib = UINib.FromName(nameof(TimeEntriesLogHeaderView), NSBundle.MainBundle);
        }

        protected TimeEntriesLogHeaderView(IntPtr handle)
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            ContentView.BackgroundColor = UIColor.White;
            DurationLabel.Font = DurationLabel.Font.GetMonospacedDigitFont();
        }

        private void updateView()
        {
            if (items.Count == 0)
                return;

            var firstItem = items.First();

            DateLabel.Text = DateToTitleString.Convert(firstItem.StartTime, now);

            var totalDuration = items.Sum(vm => vm.Duration);
            DurationLabel.Text = totalDuration.ToFormattedString(firstItem.DurationFormat);
        }
    }
}
