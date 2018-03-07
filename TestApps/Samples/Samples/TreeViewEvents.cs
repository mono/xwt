//
// TreeViewEvents.cs
//
// Author:
//       Lluis Sanchez <llsan@microsoft.com>
//
// Copyright (c) 2018 Microsoft
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
using System.Collections.Generic;
using Xwt;
using Xwt.Drawing;

namespace Samples
{
	public class TreeViewEvents : VBox
	{
		DataField<TextNode> nodeField = new DataField<TextNode> ();

		public TreeViewEvents ()
		{
			TreeStore store = new TreeStore (nodeField);
			var tree = new TreeView (store);
			tree.HeadersVisible = false;
			tree.SelectionMode = SelectionMode.None;
			tree.AnimationsEnabled = false;
			tree.ButtonPressed += (s, e) =>
			{
				// disable internal selection/dragging logic, which might collide with the mouse handling in this example
				if (e.Button == PointerButton.Left)
					e.Handled = true;
			};

			var col = new ListViewColumn () { Expands = true };
			var cellView = new ExpandableTextCellView ();
			cellView.NodeField = nodeField;
			col.Views.Add (cellView, true);
			tree.Columns.Add (col);

			var node = store.AddNode ().SetValue (nodeField, new TextNode ("Root 1"));
			var child = node.AddChild ().SetValue (nodeField, new TextNode ("Very long text. Very long text. Very long text. Very long text. Very long text. Very long text. Very long text. Very long text."));
			node = store.AddNode ().SetValue (nodeField, new TextNode ("Root 2"));
			child = node.AddChild ().SetValue (nodeField, new TextNode ("Short text. Short text. Short text."));
			child.AddChild ().SetValue (nodeField, new TextNode ("Very long text. Very long text. Very long text. Very long text. Very long text. Very long text. Very long text. Very long text."));


			PackStart (tree, true);
		}
	}

	class TextNode
	{
		public TextNode (string text)
		{
			Text = text;
		}

		public string Text { get; set; }
	}

	class ExpandableTextCellView : CanvasCellView
	{
		public IDataField<TextNode> NodeField { get; set; }

		static Image addImage = Image.FromResource ("add-simple-16.png").WithSize (16);
		static Image removeImage = Image.FromResource ("remove-simple-16.png").WithSize (16);
		static Image addImageDisabled = Image.FromResource ("add-simple-disabled-16.png").WithSize (16);
		static Image removeImageDisabled = Image.FromResource ("remove-simple-disabled-16.png").WithSize (16);

		const double MoreLinkSpacing = 3;

		Point pointerPosition;

		class ViewStatus
		{
			public bool Expanded { get; set; }
			public double LastRenderWidth;
			public double LastCalculatedHeight;
		}

		// This could also be stored in the data store. In this example we keep it in
		// an internal dictionary to clearly separate the data model from the view model.
		// This is a simple implementation, it doesn't take into account that nodes could
		// be removed
		Dictionary<TextNode, ViewStatus> viewStatus = new Dictionary<TextNode, ViewStatus> ();

		// Used to track the selection
		int selectionStart;
		int selectionEnd;
		TextNode selectionRow;
		bool dragging;

		ViewStatus GetViewStatus (TextNode node)
		{
			ViewStatus status;
			if (!viewStatus.TryGetValue (node, out status))
				status = viewStatus [node] = new ViewStatus ();
			return status;
		}

		protected override Size OnGetRequiredSize (SizeConstraint widthConstraint)
		{
			var node = GetValue (NodeField);
			var status = GetViewStatus (node);

			TextLayout layout = new TextLayout ();
			layout.Text = node.Text;
			var textSize = layout.GetSize ();

			var maxWidth = widthConstraint.IsConstrained ? widthConstraint.AvailableSize : status.LastRenderWidth;

			// When in expanded mode, the height of the row depends on the width. Since we don't know the width,
			// let's use the last width that was used for rendering.

			if (status.Expanded && maxWidth > 0 && textSize.Width > maxWidth) {
				layout.Width = maxWidth - addImage.Width - MoreLinkSpacing;
				textSize = layout.GetSize ();
			}

			status.LastCalculatedHeight = textSize.Height;

			return new Size (30, textSize.Height);
		}

 		protected override void OnDraw (Context ctx, Rectangle cellArea)
		{
			TextLayout layout = new TextLayout ();
			var node = GetValue (NodeField);
			var status = GetViewStatus (node);

			// Store the width, it will be used for calculating height in OnGetRequiredSize() when in expanded mode.
			status.LastRenderWidth = cellArea.Width;

			layout.Text = node.Text;
			var textSize = layout.GetSize ();

			// Render the selection
			if (selectionRow == node && selectionStart != selectionEnd)
				layout.SetBackground (Colors.LightBlue, Math.Min (selectionStart, selectionEnd), Math.Abs (selectionEnd - selectionStart));

			// Text doesn't fit. We need to render the expand icon

			if (textSize.Width > cellArea.Width) {
				layout.Width = Math.Max (1, cellArea.Width - addImage.Width - MoreLinkSpacing);
				if (!status.Expanded)
					layout.Trimming = TextTrimming.WordElipsis;
				else
					textSize = layout.GetSize (); // The height may have changed. We need the real height since we check it at the end of the method
				
				// Draw the text

				ctx.DrawTextLayout (layout, cellArea.X, cellArea.Y);

				// Draw the image

				var imageRect = new Rectangle (cellArea.X + layout.Width + MoreLinkSpacing, cellArea.Y, addImage.Width, addImage.Height);
				bool hover = pointerPosition != Point.Zero && imageRect.Contains (pointerPosition);
				Image icon;
				if (status.Expanded)
					icon = hover ? removeImage : removeImageDisabled;
				else
					icon = hover ? addImage : addImageDisabled;
				ctx.DrawImage (icon, imageRect.X, imageRect.Y);
			}
			else {
				ctx.DrawTextLayout (layout, cellArea.X, cellArea.Y);
			}

			// If the height required by the text is not the same as what was calculated in OnGetRequiredSize(), it means that
			// the required height has changed. In that case call QueueResize(), so that OnGetRequiredSize() is called
			// again and the row is properly resized.

			if (status.Expanded && textSize.Height != status.LastCalculatedHeight)
				QueueResize ();
		}

		void CalcLayout (out TextLayout layout, out Rectangle cellArea, out Rectangle expanderRect)
		{
			var node = GetValue (NodeField);
			var status = GetViewStatus (node);
			expanderRect = Rectangle.Zero;
			cellArea = Bounds;
			layout = new TextLayout ();
			layout.Text = node.Text;
			var textSize = layout.GetSize ();
			if (textSize.Width > cellArea.Width) {
				layout.Width = Math.Max (1, cellArea.Width - addImage.Width - MoreLinkSpacing);
				if (!status.Expanded)
					layout.Trimming = TextTrimming.WordElipsis;
				var expanderX = cellArea.Right - addImage.Width;
				if (expanderX > 0)
					expanderRect = new Rectangle (expanderX, cellArea.Y, addImage.Width, addImage.Height);
			}
		}

		protected override void OnMouseMoved (MouseMovedEventArgs args)
		{
			TextLayout layout;
			Rectangle cellArea, expanderRect;
			CalcLayout (out layout, out cellArea, out expanderRect);

			if (expanderRect != Rectangle.Zero && expanderRect.Contains (args.Position)) {
				pointerPosition = args.Position;
				QueueDraw ();
			} else if (pointerPosition != Point.Zero) {
				pointerPosition = Point.Zero;
				QueueDraw ();
			}

			var layoutSize = layout.GetSize ();
			var insideText = new Rectangle (cellArea.TopLeft, layoutSize).Contains (args.Position);
			var node = GetValue (NodeField);

			if (dragging && insideText && selectionRow == node) {
				var pos = layout.GetIndexFromCoordinates (args.Position.X - cellArea.X, args.Position.Y - cellArea.Y);
				if (pos != -1) {
					selectionEnd = pos;
					QueueDraw ();
				}
			} else {
				ParentWidget.Cursor = insideText ? CursorType.IBeam : CursorType.Arrow;
			}
		}

		protected override void OnButtonPressed (ButtonEventArgs args)
		{
			TextLayout layout;
			Rectangle cellArea, expanderRect;
			CalcLayout (out layout, out cellArea, out expanderRect);

			var node = GetValue (NodeField);
			var status = GetViewStatus (node);

			if (expanderRect != Rectangle.Zero && expanderRect.Contains (args.Position)) {
				status.Expanded = !status.Expanded;
				QueueResize ();
				return;
			}

			var pos = layout.GetIndexFromCoordinates (args.Position.X - cellArea.X, args.Position.Y - cellArea.Y);
			if (pos != -1) {
				selectionStart = selectionEnd = pos;
				selectionRow = node;
				dragging = true;
			} else
				selectionRow = null;
			
			QueueDraw ();
			
			base.OnButtonPressed (args);
		}

		protected override void OnButtonReleased (ButtonEventArgs args)
		{
			if (dragging) {
				dragging = false;
				QueueDraw ();
			}
			base.OnButtonReleased (args);
		}

		protected override void OnMouseExited ()
		{
			pointerPosition = Point.Zero;
			ParentWidget.Cursor = CursorType.Arrow;
			base.OnMouseExited ();
		}
	}
}
