// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
	[Register ("CalendarViewController")]
	partial class CalendarViewController
	{
		[Outlet]
		UIKit.UICollectionView CalendarCollectionView { get; set; }

		[Outlet]
		UIKit.UILabel CurrentDateLabel { get; set; }

		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.UIView ExtendedNavbarView { get; set; }

		[Outlet]
		UIKit.UIButton GetStartedButton { get; set; }

		[Outlet]
		UIKit.UIView OnboardingView { get; set; }

		[Outlet]
		UIKit.UILabel TimeTrackedTodayLabel { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CalendarCollectionView != null) {
				CalendarCollectionView.Dispose ();
				CalendarCollectionView = null;
			}

			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}

			if (GetStartedButton != null) {
				GetStartedButton.Dispose ();
				GetStartedButton = null;
			}

			if (OnboardingView != null) {
				OnboardingView.Dispose ();
				OnboardingView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (ExtendedNavbarView != null) {
				ExtendedNavbarView.Dispose ();
				ExtendedNavbarView = null;
			}

			if (TimeTrackedTodayLabel != null) {
				TimeTrackedTodayLabel.Dispose ();
				TimeTrackedTodayLabel = null;
			}

			if (CurrentDateLabel != null) {
				CurrentDateLabel.Dispose ();
				CurrentDateLabel = null;
			}
		}
	}
}
