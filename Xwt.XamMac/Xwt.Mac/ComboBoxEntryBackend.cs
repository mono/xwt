// 
// ComboBoxEntryBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class ComboBoxEntryBackend: ViewBackend<NSComboBox,IComboBoxEventSink>, IComboBoxEntryBackend
	{
		IListDataSource source;
		ComboDataSource tsource;
		TextEntryBackend entryBackend;
		int textColumn;
		
		public ComboBoxEntryBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new MacComboBox (EventSink, ApplicationContext);
		}
		
		protected override Size GetNaturalSize ()
		{
			var s = base.GetNaturalSize ();
			return new Size (EventSink.GetDefaultNaturalSize ().Width, s.Height);
		}

		#region IComboBoxEntryBackend implementation
		public ITextEntryBackend TextEntryBackend {
			get {
				if (entryBackend == null)
					entryBackend = new Xwt.Mac.TextEntryBackend ((MacComboBox)ViewObject);
				return entryBackend;
			}
		}
		#endregion

		#region IComboBoxBackend implementation
		public void SetViews (CellViewCollection views)
		{
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			this.source = source;
			tsource = new ComboDataSource (source);
			tsource.TextColumn = textColumn;
			Widget.UsesDataSource = true;
			Widget.DataSource = tsource;
		}

		public int SelectedRow {
			get {
				return (int) Widget.SelectedIndex;
			}
			set {
				Widget.SelectItem (value);
			}
		}
		
		public void SetTextColumn (int column)
		{
			textColumn = column;
			if (tsource != null)
				tsource.TextColumn = column;
		}
		#endregion
	}
	
	class MacComboBox: NSComboBox, IViewObject
	{
		IComboBoxEventSink eventSink;
		ITextEntryEventSink entryEventSink;
		ApplicationContext context;

		int cacheSelectionStart, cacheSelectionLength;
		bool checkMouseMovement;

		public MacComboBox (IComboBoxEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
		}
		
		public void SetEntryEventSink (ITextEntryEventSink entryEventSink)
		{
			this.entryEventSink = entryEventSink;
		}
		
		public NSView View {
			get {
				return this;
			}
		}

		public ViewBackend Backend { get; set; }
		
		public override void DidChange (NSNotification notification)
		{
			base.DidChange (notification);
			if (entryEventSink != null) {
				context.InvokeUserCode (delegate {
					entryEventSink.OnChanged ();
					entryEventSink.OnSelectionChanged ();
				});
			}
		}
		public override void KeyUp (NSEvent theEvent)
		{
			base.KeyUp (theEvent);
			HandleSelectionChanged ();
		}


		NSTrackingArea trackingArea;
		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			var viewBounds = this.Bounds;
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (viewBounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			base.RightMouseDown (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			base.RightMouseUp (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Right;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseDown (NSEvent theEvent)
		{
			base.MouseDown (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = PointerButton.Left;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonPressed (args);
			});
		}

		public override void MouseUp (NSEvent theEvent)
		{
			base.MouseUp (theEvent);
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			ButtonEventArgs args = new ButtonEventArgs ();
			args.X = p.X;
			args.Y = p.Y;
			args.Button = (PointerButton) (int)theEvent.ButtonNumber + 1;
			context.InvokeUserCode (delegate {
				eventSink.OnButtonReleased (args);
			});
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			base.MouseEntered (theEvent);
			checkMouseMovement = true;
			context.InvokeUserCode (delegate {
				eventSink.OnMouseEntered ();
			});
		}

		public override void MouseExited (NSEvent theEvent)
		{
			base.MouseExited (theEvent);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseExited ();
			});
			checkMouseMovement = false;
			HandleSelectionChanged ();
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			base.MouseMoved (theEvent);
			if (!checkMouseMovement)
				return;
			var p = ConvertPointFromView (theEvent.LocationInWindow, null);
			MouseMovedEventArgs args = new MouseMovedEventArgs ((long) TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X, p.Y);
			context.InvokeUserCode (delegate {
				eventSink.OnMouseMoved (args);
			});
			if (checkMouseMovement)
				HandleSelectionChanged ();
		}

		void HandleSelectionChanged ()
		{
			if (entryEventSink == null || CurrentEditor == null)
				return;
			if (cacheSelectionStart != CurrentEditor.SelectedRange.Location ||
			    cacheSelectionLength != CurrentEditor.SelectedRange.Length) {
				cacheSelectionStart = (int)CurrentEditor.SelectedRange.Location;
				cacheSelectionLength = (int)CurrentEditor.SelectedRange.Length;
				context.InvokeUserCode (delegate {
					entryEventSink.OnSelectionChanged ();
				});
			}
		}
	}
	
	class ComboDataSource: NSComboBoxDataSource
	{
		IListDataSource source;
		
		public int TextColumn;
		
		public ComboDataSource (IListDataSource source)
		{
			this.source = source;
		}
		
		public override NSObject ObjectValueForItem (NSComboBox comboBox, nint index)
		{
			return NSObject.FromObject (source.GetValue ((int) index, TextColumn));
		}
		
		public override nint ItemCount (NSComboBox comboBox)
		{
			return source.RowCount;
		}
	}
}

