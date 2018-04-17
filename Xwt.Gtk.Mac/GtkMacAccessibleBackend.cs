//
// GtkMacAccessibleBackend.cs
//
// Author:
//       iain holmes <iaholmes@microsoft.com>
//
// Copyright (c) Microsoft, Inc 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Linq;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xwt.Accessibility;
using Xwt.GtkBackend;

namespace Xwt.Gtk.Mac
{
	public class GtkMacAccessibleBackend : AccessibleBackend
	{
		public GtkMacAccessibleBackend ()
		{
		}

		const string XamarinPrivateAtkCocoaNSAccessibilityKey = "xamarin-private-atkcocoa-nsaccessibility";
		internal static INSAccessibility GetNSAccessibilityElement (GLib.Object o)
		{
			IntPtr handle = GtkWorkarounds.GetData (o, XamarinPrivateAtkCocoaNSAccessibilityKey);

			// The object returned could either be an NSAccessibilityElement or it might be an NSObject that implements INSAccessibility
			return Runtime.GetNSObject<NSObject> (handle, false) as INSAccessibility;
		}

		public override string Label {
			get {
				var nsa = GetNSAccessibilityElement (widget.Accessible);
				if (nsa == null) {
					return null;
				}
				return nsa.AccessibilityLabel;
			}
			set {
				var nsa = GetNSAccessibilityElement (widget.Accessible);
				if (nsa == null) {
					return;
				}
				nsa.AccessibilityLabel = value;
			}
		}

		public override Uri Uri {
			get {
				var nsa = GetNSAccessibilityElement (widget.Accessible);
				if (nsa == null) {
					return null;
				}

				var url = nsa.AccessibilityUrl;
				return new Uri (url.AbsoluteString);
			}
			set {
				var nsa = GetNSAccessibilityElement (widget.Accessible);
				if (nsa == null) {
					return;
				}

				nsa.AccessibilityUrl = new NSUrl (value.AbsoluteUri);
			}
		}

		public override void AddChild (object nativeChild)
		{
			var nsa = GetNSAccessibilityElement (widget.Accessible);
			if (nsa == null) {
				return;
			}

			var parent = nsa as NSAccessibilityElement;
			if (parent != null) {
				var child = nativeChild as NSAccessibilityElement;
				if (child == null) {
					throw new ArgumentException ($"{nativeChild.GetType ().ToString ()} - should be NSAccessibilityElement", nameof (nativeChild));
				}
				parent.AccessibilityAddChildElement (child);
			}
		}

		public override void RemoveChild (object nativeChild)
		{
			var nsa = GetNSAccessibilityElement (widget.Accessible);
			if (nsa == null) {
				return;
			}

			var parent = nsa as NSAccessibilityElement;

			if (parent != null) {
				parent.AccessibilityChildren = parent.AccessibilityChildren.Where(c => c != nativeChild).ToArray ();
			}
		}

		public override void RemoveAllChildren ()
		{
			var nsa = GetNSAccessibilityElement (widget.Accessible);
			if (nsa == null) {
				return;
			}

			nsa.AccessibilityChildren = null;
		}
	}
}
