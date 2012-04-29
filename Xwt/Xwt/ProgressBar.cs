// 
// ProgressBar.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Anrdres G. Aragoneses
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
using Xwt.Drawing;

namespace Xwt
{
	public class ProgressBar : Widget
	{
		EventHandler clicked;
		ButtonStyle style = ButtonStyle.Normal;
		ButtonType type = ButtonType.Normal;
		string label;
		ContentPosition imagePosition = ContentPosition.Left;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IProgressBarEventSink
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				((IProgressBarBackend)Backend).SetButtonStyle (((ProgressBar)Parent).style);
			}
			
			public void OnClicked ()
			{
				((ProgressBar)Parent).OnClicked (EventArgs.Empty);
			}
		}
		
		static ProgressBar ()
		{
			MapEvent (ButtonEvent.Clicked, typeof(Button), "OnClicked");
		}
		
		public ProgressBar ()
		{
		}
		
		public ProgressBar (string label): this ()
		{
			Label = label;
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IProgressBarBackend Backend {
			get { return (IProgressBarBackend) BackendHost.Backend; }
		}
		
		public string Label {
			get { return label; }
			set {
				label = value;
				Backend.SetContent (label, imagePosition);
				OnPreferredSizeChanged ();
			}
		}
		
		public ContentPosition ImagePosition {
			get { return imagePosition; }
			set {
				imagePosition = value;
				Backend.SetContent (label, imagePosition); 
				OnPreferredSizeChanged ();
			}
		}
		
		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}
		
		public ButtonType Type {
			get { return type; }
			set {
				type = value;
				Backend.SetButtonType (type);
				OnPreferredSizeChanged ();
			}
		}
		
		protected virtual void OnClicked (EventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
		
		public event EventHandler Clicked {
			add {
				BackendHost.OnBeforeEventAdd (ButtonEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				BackendHost.OnAfterEventRemove (ButtonEvent.Clicked, clicked);
			}
		}
	}
}

