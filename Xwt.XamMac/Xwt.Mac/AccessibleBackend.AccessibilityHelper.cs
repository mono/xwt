using System;
using Xwt.Backends;

namespace Xwt.Mac
{
	public partial class AccessibleBackend : IAccessibleBackend
	{
		static AccerssibilityHelper a11yHelper;
		static AccessibleBackend ()
		{
			a11yHelper = new AccerssibilityHelper ();
		}

		public void MakeAnnouncement (string message, bool polite = false)
		{
			a11yHelper.MakeAnnouncement (message, polite);
		}

		public bool AccessibilityInUse => a11yHelper.AccessibilityInUse;

		public event EventHandler AccessibilityInUseChanged
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
