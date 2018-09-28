//
// SearchTextEntryBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2018 (c) Microsoft Corporation
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
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class SearchTextEntryBackend : TextEntryBackend, ISearchTextEntryBackend
	{
		public override void Initialize()
		{
			var view = new CustomSearchTextField(EventSink, ApplicationContext);
			ViewObject = new CustomAlignedContainer(EventSink, ApplicationContext, view) { DrawsBackground = false, ExpandVertically = true };
			base.Initialize();
		}
	}

	class CustomSearchTextField : NSSearchField, IViewObject
	{
		ITextEntryEventSink eventSink;
		ApplicationContext context;
		#pragma warning disable CS0414 // The private field is assigned but its value is never used
		CustomCell cell;
		#pragma warning disable CS0414

		public CustomSearchTextField(ITextEntryEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
			this.Cell = cell = new CustomCell
			{
				Bezeled = true,
				Editable = true,
				EventSink = eventSink,
				Context = context,
			};
		}

		public NSView View
		{
			get
			{
				return this;
			}
		}

		public ViewBackend Backend { get; set; }

		public override void DidChange(NSNotification notification)
		{
			base.DidChange(notification);
			context.InvokeUserCode(delegate
			{
				eventSink.OnChanged();
				eventSink.OnSelectionChanged();
			});
		}

		public override string StringValue
		{
			get { return base.StringValue; }
			set
			{
				if (base.StringValue != value)
				{
					base.StringValue = value;
					context.InvokeUserCode(delegate
					{
						eventSink.OnChanged();
						eventSink.OnSelectionChanged();
					});
				}
			}
		}

		class CustomCell : NSSearchFieldCell
		{
			NSTextView editor;
			NSObject selChangeObserver;
			public ApplicationContext Context
			{
				get; set;
			}

			public ITextEntryEventSink EventSink
			{
				get; set;
			}

			public CustomCell()
			{

			}

			protected CustomCell(IntPtr ptr) : base(ptr)
			{
			}

			/// <summary>
			/// Like what happens for the ios designer, AppKit can sometimes clone the native `NSTextFieldCell` using the Copy (NSZone)
			/// method. We *need* to ensure we can create a new managed wrapper for the cloned native object so we need the IntPtr
			/// constructor. NOTE: By keeping this override in managed we ensure the new wrapper C# object is created ~immediately,
			/// which makes it easier to debug issues.
			/// </summary>
			/// <returns>The copy.</returns>
			/// <param name="zone">Zone.</param>
			public override NSObject Copy(NSZone zone)
			{
				// Don't remove this override because the comment on this explains why we need this!
				var newCell = (CustomCell)base.Copy(zone);
				newCell.editor = editor;
				newCell.selChangeObserver = selChangeObserver;
				newCell.Context = Context;
				newCell.EventSink = EventSink;
				return newCell;
			}

			public override NSTextView FieldEditorForView(NSView aControlView)
			{
				if (editor == null)
				{
					editor = new CustomTextFieldCellEditor
					{
						Context = this.Context,
						EventSink = this.EventSink,
						FieldEditor = true,
						Editable = true,
					};
					selChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSTextViewDidChangeSelectionNotification"), HandleSelectionDidChange, editor);
				}
				return editor;
			}

			void HandleSelectionDidChange(NSNotification notif)
			{
				Context.InvokeUserCode(EventSink.OnSelectionChanged);
			}

			public override void DrawInteriorWithFrame(CGRect cellFrame, NSView inView)
			{
				base.DrawInteriorWithFrame(VerticalCenteredRectForBounds(cellFrame), inView);
			}

			public override void EditWithFrame(CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, NSEvent theEvent)
			{
				base.EditWithFrame(VerticalCenteredRectForBounds(aRect), inView, editor, delegateObject, theEvent);
			}

			public override void SelectWithFrame(CGRect aRect, NSView inView, NSText editor, NSObject delegateObject, nint selStart, nint selLength)
			{
				base.SelectWithFrame(VerticalCenteredRectForBounds(aRect), inView, editor, delegateObject, selStart, selLength);
			}

			CGRect VerticalCenteredRectForBounds(CGRect aRect)
			{
				// multiline entries should always align on top
				if (!UsesSingleLineMode)
					return aRect;

				var textHeight = CellSizeForBounds(aRect).Height;
				var offset = (aRect.Height - textHeight) / 2;
				if (offset <= 0) // do nothing if the frame is too small
					return aRect;
				var rect = new Rectangle(aRect.X, aRect.Y, aRect.Width, aRect.Height).Inflate(0.0, -offset);
				return rect.ToCGRect();
			}
		}
	}
}