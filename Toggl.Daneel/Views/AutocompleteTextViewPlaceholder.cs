﻿using System;
using Foundation;
using MvvmCross.Plugin.Color.Platforms.Ios;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(AutocompleteTextViewPlaceholder))]
    public sealed class AutocompleteTextViewPlaceholder : UILabel
    {
        public AutocompleteTextViewPlaceholder(IntPtr handle) 
            : base(handle)
        {
        }

        public void ConfigureWith(UITextView textView)
        {
            Font = UIFont.SystemFontOfSize(textView.Font.PointSize);
            TextColor = Color.StartTimeEntry.Placeholder.ToNativeColor();
            Text = Resources.StartTimeEntryPlaceholder;
            TextAlignment = UITextAlignment.Left;
            TranslatesAutoresizingMaskIntoConstraints = false;

            // AttributedText at this point has a length of 1, which is
            // a strange behaviour for an otherwise empty TextView.
            // It is set here to an empty string so that the initial visibility  
            // of the placeholder can be correctly inferred.
            textView.AttributedText = new NSAttributedString("");
        }

        public void UpdateVisibility(UITextView textView)
        {
            var hasText = textView.AttributedText?.Length > 0;
            var isInMultiStageEdit = textView.MarkedTextRange != null;

            Hidden = hasText || isInMultiStageEdit;
        }
    }
}
