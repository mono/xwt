//
// IEmbeddedWidgetBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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

namespace Xwt
{
	/// <summary>
	/// Specifies how a wrapped native wiget fits into the Xwt layout system.
	/// </summary>
	public enum NativeWidgetSizing
	{
		/// <summary>
		/// The Widget reports no preferred size. A size must either be explicitly set or assigned by the parent.
		/// </summary>
		External,

		/// <summary>
		/// The Widget reports the preferred size determined by the default Widget backend.
		/// </summary>
		DefaultPreferredSize
	}
}

namespace Xwt.Backends
{
	public interface IEmbeddedWidgetBackend: IWidgetBackend
	{
		void SetContent (object nativeWidget);
	}

	[BackendType (typeof(IEmbeddedWidgetBackend))]
	internal class EmbeddedNativeWidget: Widget
	{
		object nativeWidget;
		Widget sourceWidget;
		NativeWidgetSizing sizing;

		class EmbeddedNativeWidgetBackendHost: WidgetBackendHost<EmbeddedNativeWidget,IEmbeddedWidgetBackend>
		{
			protected override void OnBackendCreated ()
			{
				Backend.SetContent (Parent.nativeWidget);
				base.OnBackendCreated ();
			}
		}

		protected override Xwt.Backends.BackendHost CreateBackendHost ()
		{
			return new EmbeddedNativeWidgetBackendHost ();
		}

		public void Initialize (object nativeWidget, Widget sourceWidget, NativeWidgetSizing preferredSizing)
		{
			this.nativeWidget = nativeWidget;
			this.sourceWidget = sourceWidget;
			this.sizing = preferredSizing;
		}
		
		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (sourceWidget != null)
				return sourceWidget.Surface.GetPreferredSize (widthConstraint, heightConstraint);
			else
				return sizing == NativeWidgetSizing.External ? Size.Zero : base.OnGetPreferredSize (widthConstraint, heightConstraint);
		}
	}
}

