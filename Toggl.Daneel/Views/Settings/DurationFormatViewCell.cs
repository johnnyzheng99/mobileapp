using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Cells;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Settings
{
    public sealed partial class DurationFormatViewCell : BaseTableViewCell<SelectableDurationFormatViewModel>
    {
        public static readonly string Identifier = nameof(DurationFormatViewCell);

        public static readonly NSString Key = new NSString(nameof(DurationFormatViewCell));
        public static readonly UINib Nib;

        static DurationFormatViewCell()
        {
            Nib = UINib.FromName(nameof(DurationFormatViewCell), NSBundle.MainBundle);
        }

        public DurationFormatViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            DurationFormatLabel.Text = string.Empty;
            SelectedImageView.Hidden = true;
        }

        protected override void UpdateView()
        {
            DurationFormatLabel.Text = DurationFormatToString.Convert(Item.DurationFormat);
            SelectedImageView.Hidden = !Item.Selected;
        }
    }
}
