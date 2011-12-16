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

namespace Xwt
{
	public class Frame: Widget
	{
		Widget child;
		WidgetSpacing borderWidth;
		WidgetSpacing padding;
		FrameType type;
		
		protected new class EventSink: Widget.EventSink, IFrameEventSink
		{
			public override void OnSpacingChanged (WidgetSpacing source)
			{
				Frame f = (Frame)Parent;
				if (source == f.borderWidth) {
					f.Backend.SetBorderSize (source.Left, source.Right, source.Top, source.Bottom);
					f.OnPreferredSizeChanged ();
				}
				else if (source == f.padding) {
					f.Backend.SetPadding (source.Left, source.Right, source.Top, source.Bottom);
					f.OnPreferredSizeChanged ();
				}
				else
					base.OnSpacingChanged (source);
			}
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IFrameBackend Backend {
			get { return (IFrameBackend)base.Backend; }
		}
		
		public Frame ()
		{
			borderWidth = new WidgetSpacing (this.WidgetEventSink);
			padding = new WidgetSpacing (this.WidgetEventSink);
		}
		
		public Frame (Widget content): this ()
		{
			Content = content;
		}
		
		[DefaultValue (FrameType.WidgetBox)]
		public FrameType Type {
			get { return type; }
			set { type = value; Backend.SetFrameType (type); }
		}
		
		[DefaultValue ("")]
		public string Label {
			get { return Backend.Label ?? ""; }
			set { Backend.Label = value ?? ""; }
		}
		
		public WidgetSpacing Padding {
			get { return padding; }
		}
		
		public WidgetSpacing BorderWidth {
			get { return borderWidth; }
		}
		
		public Color BorderColor {
			get { return Backend.BorderColor; }
			set { Backend.BorderColor = value; }
		}
		
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

