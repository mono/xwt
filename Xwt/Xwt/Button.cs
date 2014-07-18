// 
// Button.cs
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
using Xwt.Drawing;
using System.ComponentModel;

namespace Xwt
{
	[BackendType (typeof(IButtonBackend))]
	public class Button: Widget
	{
		EventHandler clicked;
		ButtonStyle style = ButtonStyle.Normal;
		ButtonType type = ButtonType.Normal;
		Image image;
		string label;
		bool useMnemonic = true;
		ContentPosition imagePosition = ContentPosition.Left;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IButtonEventSink
		{
			protected override void OnBackendCreated ()
			{
				base.OnBackendCreated ();
				((IButtonBackend)Backend).SetButtonStyle (((Button)Parent).style);
			}
			
			public void OnClicked ()
			{
				((Button)Parent).OnClicked (EventArgs.Empty);
			}
		}
		
		static Button ()
		{
			MapEvent (ButtonEvent.Clicked, typeof(Button), "OnClicked");
		}
		
		public Button ()
		{
		}
		
		public Button (string label)
		{
			VerifyConstructorCall (this);
			Label = label;
		}
		
		public Button (Image img, string label)
		{
			VerifyConstructorCall (this);
			Label = label;
			Image = img;
		}
		
		public Button (Image img)
		{
			VerifyConstructorCall (this);
			Image = img;
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IButtonBackend Backend {
			get { return (IButtonBackend) BackendHost.Backend; }
		}
		
		[DefaultValue ("")]
		public string Label {
			get { return label ?? ""; }
			set {
				label = value;
				Backend.SetContent (label, UseMnemonic, image != null ? image.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null, imagePosition);
				OnPreferredSizeChanged ();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Xwt.Button"/> uses a mnemonic.
		/// </summary>
		/// <value><c>true</c> if it uses a mnemonic; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// When set to true, the character after the first underscore character in the Label property value is
		/// interpreted as the mnemonic for that Label.
		/// </remarks>
		[DefaultValue(true)]
		public bool UseMnemonic { 
			get { return useMnemonic; }
			set
			{ 
				if (useMnemonic == value)
					return;
				Backend.SetContent (label, value, image != null ? image.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null, imagePosition);
				useMnemonic = value;
			}
		}

		[DefaultValue (null)]
		public Image Image {
			get { return image; }
			set {
				image = value;
				Backend.SetContent (label, UseMnemonic, image != null ? image.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null, imagePosition);
				OnPreferredSizeChanged ();
			}
		}
		
		[DefaultValue (ContentPosition.Left)]
		public ContentPosition ImagePosition {
			get { return imagePosition; }
			set {
				imagePosition = value;
				Backend.SetContent (label, UseMnemonic, image != null ? image.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null, imagePosition);
				OnPreferredSizeChanged ();
			}
		}
		
		[DefaultValue (ButtonStyle.Normal)]
		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}
		
		[DefaultValue (ButtonType.Normal)]
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

