using System;
using System.Runtime.InteropServices;
using AppKit;
using CoreFoundation;
using Foundation;
using ObjCRuntime;

namespace Xwt.Mac
{
	public class AccerssibilityHelper
	{
		public event EventHandler AccessibilityInUseChanged;

		static bool a11yHelperInitialized;
		static AccerssibilityHelper a11yHelperInstance;
		public void InitializeAccessibilityHelper()
		{
			// Don't swizzle twice if we have both XamMac and GtkMac running
			if (a11yHelperInstance != null)
				return;

			a11yHelperInstance = this;
			// TODO: Test if this works with VSM swizzle
			SwizzleNSApplicationAccessibilitySetter();
		}

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr class_getInstanceMethod(IntPtr classHandle, IntPtr Selector);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr method_getImplementation(IntPtr method);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern IntPtr imp_implementationWithBlock(ref BlockLiteral block);

		[DllImport("/usr/lib/libobjc.dylib")]
		private static extern void method_setImplementation(IntPtr method, IntPtr imp);

		[MonoNativeFunctionWrapper]
		delegate void AccessibilitySetValueForAttributeDelegate(IntPtr self, IntPtr selector, IntPtr valueHandle, IntPtr attributeHandle);
		delegate void SwizzledAccessibilitySetValueForAttributeDelegate(IntPtr block, IntPtr self, IntPtr valueHandle, IntPtr attributeHandle);

		static IntPtr originalAccessibilitySetValueForAttributeMethod;
		void SwizzleNSApplicationAccessibilitySetter()
		{
			// Swizzle accessibilitySetValue:forAttribute: so that we can detect when VoiceOver gets enabled
			var nsApplicationClassHandle = Class.GetHandle("NSApplication");

			// This happens if GtkMac is loaded before XamMac
			if (nsApplicationClassHandle == IntPtr.Zero)
				return;

			var accessibilitySetValueForAttributeSelector = Selector.GetHandle("accessibilitySetValue:forAttribute:");

			var accessibilitySetValueForAttributeMethod = class_getInstanceMethod(nsApplicationClassHandle, accessibilitySetValueForAttributeSelector);
			originalAccessibilitySetValueForAttributeMethod = method_getImplementation(accessibilitySetValueForAttributeMethod);

			var block = new BlockLiteral();

			SwizzledAccessibilitySetValueForAttributeDelegate d = accessibilitySetValueForAttribute;
			block.SetupBlock(d, null);
			var imp = imp_implementationWithBlock(ref block);
			method_setImplementation(accessibilitySetValueForAttributeMethod, imp);

			accessibilityInUse = CFPreferences.GetAppBooleanValue("voiceOverOnOffKey", "com.apple.universalaccess");
			a11yHelperInitialized = true;
		}

		[MonoPInvokeCallback(typeof(SwizzledAccessibilitySetValueForAttributeDelegate))]
		static void accessibilitySetValueForAttribute(IntPtr block, IntPtr self, IntPtr valueHandle, IntPtr attributeHandle)
		{
			var d = Marshal.GetDelegateForFunctionPointer(originalAccessibilitySetValueForAttributeMethod, typeof(AccessibilitySetValueForAttributeDelegate));
			d.DynamicInvoke(self, Selector.GetHandle("accessibilitySetValue:forAttribute:"), valueHandle, attributeHandle);

			var val = (NSNumber)ObjCRuntime.Runtime.GetNSObject(valueHandle);

			bool previousValue = accessibilityInUse;
			accessibilityInUse = val.BoolValue;
			if (accessibilityInUse != previousValue)
				a11yHelperInstance.OnAccessibilityInUseChanged(a11yHelperInstance, EventArgs.Empty);
		}

		void OnAccessibilityInUseChanged(object sender, EventArgs eventArgs)
		{
			AccessibilityInUseChanged?.Invoke(sender, eventArgs);
		}

		static bool accessibilityInUse;
		public bool AccessibilityInUse
		{
			get
			{
				if (!a11yHelperInitialized)
					InitializeAccessibilityHelper();

				return accessibilityInUse;
			}
		}

		public void MakeAnnouncement(string message, bool polite = false)
		{
			if (!a11yHelperInitialized)
				return;

			var nsObject = NSApplication.SharedApplication?.AccessibilityFocusedWindow;
			if (nsObject == null)
				return;
			using (var msg = new NSString(message))
			using (var dictionary = new NSDictionary(NSAccessibilityNotificationUserInfoKeys.AnnouncementKey, msg,
					NSAccessibilityNotificationUserInfoKeys.PriorityKey, polite ? NSAccessibilityPriorityLevel.Medium : NSAccessibilityPriorityLevel.High))
			{
				NSAccessibility.PostNotification(nsObject, NSAccessibilityNotifications.AnnouncementRequestedNotification, dictionary);
			}
		}
	}
}
