// 
// Frame.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Backends;
using System.ComponentModel;
using Xwt.Drawing;
using System.Windows.Markup;

namespace Xwt
{
	[BackendType (typeof(IFrameBackend))]
	[ContentProperty("Content")]
	public class Frame: Widget
	{
		Widget child;
		WidgetSpacing borderWidth;
		WidgetSpacing padding;
		FrameType type;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IFrameEventSink
		{
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IFrameBackend Backend {
			get { return (IFrameBackend)BackendHost.Backend; }
		}
		
		public Frame ()
		{
		}

		[Obsolete ("Use Xwt.FrameBox")]
		public Frame (FrameType frameType)
		{
			VerifyConstructorCall (this);
			Type = frameType;
		}
		
		public Frame (Widget content)
		{
			VerifyConstructorCall (this);
			Content = content;
		}

		[Obsolete ("Use Xwt.FrameBox")]
		public Frame (Widget content, FrameType frameType)
		{
			VerifyConstructorCall (this);
			Type = frameType;
			Content = content;
		}
		
		[Obsolete ("Use Xwt.FrameBox")]
		[DefaultValue (FrameType.WidgetBox)]
		public FrameType Type {
			get { return type; }
			set { type = value; Backend.SetFrameType (type); }
		}
		
		[DefaultValue (null)]
		public string Label {
			get { return Backend.Label; }
			set { Backend.Label = value; }
		}
		
		public WidgetSpacing Padding {
			get { return padding; }
			set {
				padding = value;
				UpdatePadding ();
			}
		}

		[DefaultValue (0d)]
		public double PaddingLeft {
			get { return padding.Left; }
			set {
				padding.Left = value;
				UpdatePadding (); 
			}
		}

		[DefaultValue (0d)]
		public double PaddingRight {
			get { return padding.Right; }
			set {
				padding.Right = value;
				UpdatePadding (); 
			}
		}

		[DefaultValue (0d)]
		public double PaddingTop {
			get { return padding.Top; }
			set {
				padding.Top = value;
				UpdatePadding (); 
			}
		}

		[DefaultValue (0d)]
		public double PaddingBottom {
			get { return padding.Bottom; }
			set {
				padding.Bottom = value;
				UpdatePadding (); 
			}
		}

		void UpdatePadding ()
		{
			Backend.SetPadding (padding.Left, padding.Right, padding.Top, padding.Bottom);
			OnPreferredSizeChanged ();
		}
		
		[Obsolete ("Use Xwt.FrameBox")]
		public WidgetSpacing BorderWidth {
			get { return borderWidth; }
			set {
				borderWidth = value;
				UpdateBorderWidth ();
			}
		}

		[Obsolete ("Use Xwt.FrameBox")]
		[DefaultValue (0d)]
		public double BorderWidthLeft {
			get { return borderWidth.Left; }
			set {
				borderWidth.Left = value;
				UpdateBorderWidth (); 
			}
		}

		[Obsolete ("Use Xwt.FrameBox")]
		[DefaultValue (0d)]
		public double BorderWidthRight {
			get { return borderWidth.Right; }
			set {
				borderWidth.Right = value;
				UpdateBorderWidth (); 
			}
		}

		[Obsolete ("Use Xwt.FrameBox")]
		[DefaultValue (0d)]
		public double BorderWidthTop {
			get { return borderWidth.Top; }
			set {
				borderWidth.Top = value;
				UpdateBorderWidth (); 
			}
		}

		[Obsolete ("Use Xwt.FrameBox")]
		[DefaultValue (0d)]
		public double BorderWidthBottom {
			get { return borderWidth.Bottom; }
			set {
				borderWidth.Bottom = value;
				UpdateBorderWidth (); 
			}
		}

		void UpdateBorderWidth ()
		{
			Backend.SetBorderSize (borderWidth.Left, borderWidth.Right, borderWidth.Top, borderWidth.Bottom);
			OnPreferredSizeChanged ();
		}

		[Obsolete ("Use Xwt.FrameBox")]
		public Color BorderColor {
			get { return Backend.BorderColor; }
			set { Backend.BorderColor = value; }
		}

		/// <summary>
		/// Removes all children of the Frame
		/// </summary>
		public void Clear ()
		{
			Content = null;
		}

		[DefaultValue (null)]
		public new Widget Content {
			get { return child; }
			set {
				if (child != null)
					UnregisterChild (child);
				child = value;
				if (child != null)
					RegisterChild (child);
				Backend.SetContent ((IWidgetBackend)GetBackend (child));
				OnPreferredSizeChanged ();
			}
		}
	}
	
	public enum FrameType
	{
		Custom,
		WidgetBox
	}
}

