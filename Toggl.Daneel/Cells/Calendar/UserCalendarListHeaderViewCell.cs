using System;
using Foundation;
using UIKit;

namespace Toggl.Daneel.Cells.Calendar
{
    public sealed partial class UserCalendarListHeaderViewCell : BaseTableHeaderFooterView<string>
    {
        public static readonly string Identifier = nameof(UserCalendarListHeaderViewCell);
        public static readonly NSString Key = new NSString(nameof(UserCalendarListHeaderViewCell));
        public static readonly UINib Nib;

        static UserCalendarListHeaderViewCell()
        {
            Nib = UINib.FromName(nameof(UserCalendarListHeaderViewCell), NSBundle.MainBundle);
        }

        protected UserCalendarListHeaderViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        protected override void UpdateView()
        {
            TitleLabel.Text = Item;
        }
    }
}
