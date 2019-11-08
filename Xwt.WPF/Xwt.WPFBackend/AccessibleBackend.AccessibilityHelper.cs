using System;
using Xwt.Backends;

#if NET47
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
#endif

namespace Xwt.WPFBackend
{
	partial class AccessibleBackend : IAccessibleBackend
	{
		public bool AccessibilityInUse {
			get {
#if NET47
				// AutomationEvents.AutomationFocusChanged returns false sometimes when the application loses
				// accessibility focus. So LiveRegionChanged plays fallback role here (it comes as true more reliable).
				return AutomationPeer.ListenerExists (AutomationEvents.AutomationFocusChanged)
					|| AutomationPeer.ListenerExists (AutomationEvents.LiveRegionChanged);
#else
				return false;
#endif
			}
		}

		public void MakeAnnouncement (string message, bool polite = false)
		{
#if NET47
			string previousAccessibleLabel = Label;

			var announcementResetTimer = new DispatcherTimer ();
			announcementResetTimer.Interval = new TimeSpan (0, 0, 5);
			announcementResetTimer.Tick += (sender, args) => {
				element?.Dispatcher.BeginInvoke ((Action) (() => {
					AutomationProperties.SetLiveSetting (element, AutomationLiveSetting.Off);
					Label = previousAccessibleLabel;
				}), DispatcherPriority.Normal);

				announcementResetTimer.Stop ();
			};

			element.Dispatcher.BeginInvoke ((Action) (() => {
				AutomationProperties.SetLiveSetting (element, polite ? AutomationLiveSetting.Polite : AutomationLiveSetting.Assertive);

				Label = message;
				var peer = FrameworkElementAutomationPeer.FromElement (element);
				if (peer != null) {
					peer.RaiseAutomationEvent (AutomationEvents.LiveRegionChanged);

					// HACK: Giving some time to announce the message
					announcementResetTimer.Start ();
				}
			}), DispatcherPriority.Normal);
#else
			return;
#endif
		}

		// TODO: AccessibilityInUseChanged event would require listening for a top-level window' events in WndProc
		public event EventHandler AccessibilityInUseChanged;
	}
}
