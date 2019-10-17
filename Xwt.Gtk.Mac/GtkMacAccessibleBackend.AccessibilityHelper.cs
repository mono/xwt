using System;
using Xwt.GtkBackend;
using Xwt.Mac;

namespace Xwt.Gtk.Mac
{
	public partial class GtkMacAccessibleBackend : AccessibleBackend
	{
		static AccerssibilityHelper a11yHelper;
		static GtkMacAccessibleBackend ()
		{
			a11yHelper = new AccerssibilityHelper ();
		}

		public override void MakeAnnouncement (string message, bool polite = false)
		{
			a11yHelper.MakeAnnouncement (message, polite);
		}

		public override bool AccessibilityInUse => a11yHelper.AccessibilityInUse;

		public override event EventHandler AccessibilityInUseChanged
		{
			add
			{
				a11yHelper.AccessibilityInUseChanged += value;
			}
			remove
			{
				a11yHelper.AccessibilityInUseChanged -= value;
			}
		}
	}
}
