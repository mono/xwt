//
// PanedBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xwt.Backends;
using Xwt.Engine;
using Orientation = Xwt.Backends.Orientation;

namespace Xwt.WPFBackend
{
	public class PanedBackend
		: WidgetBackend, IPanedBackend
	{
		public void Initialize (Orientation dir)
		{
			this.direction = dir;

			Grid = new Grid();
		}

		// TODO
		public double Position
		{
			get;
			set;
		}

		public void SetPanel (int panel, IWidgetBackend widget, bool resize)
		{
			panel--;

			var element = (UIElement) widget.NativeWidget;

			if (this.direction == Orientation.Horizontal) {
				if (panel > 0 || Grid.ColumnDefinitions.Count > 0) {
					GridSplitter splitter = new GridSplitter {
						ResizeDirection = GridResizeDirection.Columns,
						VerticalAlignment = VerticalAlignment.Stretch,
						HorizontalAlignment = HorizontalAlignment.Center,
						Width = SplitterSize
					};
					splitter.DragDelta += SplitterOnDragDelta;

					Grid.ColumnDefinitions.Insert (panel, new ColumnDefinition { Width = GridLength.Auto });
					Grid.SetColumn (splitter, panel++);
					Grid.Children.Add (splitter);
				}

				ColumnDefinition definition = new ColumnDefinition();
				definition.Width = new GridLength (1, (resize) ? GridUnitType.Star : GridUnitType.Auto);
				Grid.ColumnDefinitions.Insert (panel, definition);

				Grid.SetColumn (element, panel);
			} else {
				if (panel > 0 || Grid.RowDefinitions.Count > 0) {
					GridSplitter splitter = new GridSplitter {
						ResizeDirection = GridResizeDirection.Rows,
						HorizontalAlignment = HorizontalAlignment.Stretch,
						VerticalAlignment = VerticalAlignment.Center,
						Height = SplitterSize
					};
					splitter.DragDelta += SplitterOnDragDelta;

					Grid.RowDefinitions.Insert (panel, new RowDefinition { Height = GridLength.Auto });
					Grid.SetRow (splitter, panel++);
					Grid.Children.Add (splitter);
				}

				RowDefinition definition = new RowDefinition();
				definition.Height = new GridLength (1, (resize) ? GridUnitType.Star : GridUnitType.Auto);
				Grid.RowDefinitions.Insert (panel, definition);

				Grid.SetRow (element, panel);
			}

			Grid.Children.Add (element);
		}

		public void UpdatePanel (int panel, bool resize)
		{
			int panelCount = (this.direction == Orientation.Horizontal)
			                 	? Grid.ColumnDefinitions.Count
			                 	: Grid.RowDefinitions.Count;

			if (panel > panelCount)
				return;

			panel--;
			panel *= 2; // adjust for splitters

			if (this.direction == Orientation.Horizontal) {
				var column = Grid.ColumnDefinitions [panel];
				column.Width = new GridLength (1, (resize) ? GridUnitType.Star : GridUnitType.Auto);
			} else {
				var row = Grid.RowDefinitions [panel];
				row.Height = new GridLength (1, (resize) ? GridUnitType.Star : GridUnitType.Auto);
			}
		}

		public void RemovePanel (int panel)
		{
			panel--;
			panel *= 2; // adjust for splitters

			if (this.direction == Orientation.Horizontal) {
				Grid.ColumnDefinitions.RemoveAt (panel);
				if (panel > 0)
					Grid.ColumnDefinitions.RemoveAt (panel); // splitter
			} else {
				Grid.RowDefinitions.RemoveAt (panel);
				if (panel > 0)
					Grid.RowDefinitions.RemoveAt (panel); // splitter
			}

			RemoveElementsForPanel (panel);
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);

			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged:
					this.reportPositionChanged = true;
					break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);

			if (eventId is PanedEvent) {
				switch ((PanedEvent)eventId) {
				case PanedEvent.PositionChanged:
					this.reportPositionChanged = false;
					break;
				}
			}
		}

		private const int SplitterSize = 4;
		private Orientation direction;
		private bool reportPositionChanged;

		private Grid Grid
		{
			get { return (Grid) Widget; }
			set { Widget = value; }
		}

		private void SplitterOnDragDelta (object sender, DragDeltaEventArgs e)
		{
			if (this.reportPositionChanged)
				Toolkit.Invoke (((IPanedEventSink)EventSink).OnPositionChanged);
		}

		private void RemoveElementsForPanel (int panel)
		{
			bool previous = (panel > 0);

			Func<UIElement, bool> predicate;
			if (this.direction == Orientation.Horizontal) {
				predicate = e => {
					int column = Grid.GetColumn (e);
					return (column == panel || (previous && column - 1 == panel));
				};
			} else {
				predicate = e => {
					int row = Grid.GetRow (e);
					return (row == panel || (previous && row - 1 == panel));
				};
			}
				
			foreach (UIElement element in Grid.Children.OfType<UIElement>().Where (predicate))
				Grid.Children.Remove (element);
		}
	}
}