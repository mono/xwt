//
// BasicChart.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2005 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.Components.Chart
{
	public class BasicChart: Canvas
	{
		double startX, endX;
		double startY, endY;
		double left, top, width, height;
		int minTickStep = 5;
		bool xrangeChanged = true;
		bool yrangeChanged = true;
		
		bool autoStartX, autoStartY;
		bool autoEndX, autoEndY;
				
		double originX;
		double originY;
		bool reverseXAxis;
		bool reverseYAxis;
		
		double AreaBorderWidth = 1;
		int AutoScaleMargin = 3;
		int MinLabelGapX = 3;
		int MinLabelGapY = 1;
		BackgroundDisplay backgroundDisplay = BackgroundDisplay.Gradient;
		Color backroundColor = new Color (0.9, 0.9, 1);
		
		ArrayList series = new ArrayList ();
		ArrayList axis = new ArrayList ();
		ArrayList cursors = new ArrayList ();
		
		bool enableSelection;
		bool draggingCursor;
		ChartCursor activeCursor;
		
		ChartCursor selectionStart;
		ChartCursor selectionEnd;
		
		Font chartFont = Font.FromName ("Tahoma 8");

		
		public BasicChart ()
		{
			selectionStart = new ChartCursor ();
			selectionStart.Visible = false;
			selectionEnd = new ChartCursor ();
			selectionEnd.Visible = false;
			AddCursor (selectionStart, AxisDimension.X);
			AddCursor (selectionEnd, AxisDimension.X);
			selectionStart.ValueChanged += new EventHandler (OnSelectionCursorChanged);
			selectionEnd.ValueChanged += new EventHandler (OnSelectionCursorChanged);
		}
		
		public event EventHandler SelectionChanged;
		
		public bool AllowSelection {
			get {
				return enableSelection;
			}
			set {
				enableSelection = value;
				if (!enableSelection) {
					selectionStart.Visible = false;
					selectionEnd.Visible = false;
				}
			}
		}
		
		public ChartCursor SelectionStart {
			get { return selectionStart; }
		}
		
		public ChartCursor SelectionEnd {
			get { return selectionEnd; }
		}
		
		public ChartCursor ActiveCursor {
			get { return activeCursor; }
		}
		
		public bool ReverseXAxis {
			get { return reverseXAxis; }
			set { reverseXAxis = value; QueueDraw (); }
		}
		
		public bool ReverseYAxis {
			get { return reverseYAxis; }
			set { reverseYAxis = value; QueueDraw (); }
		}
		
		public double OriginX {
			get { return originX; }
			set {
				xrangeChanged = true;
				originX = value;
				OnSerieChanged ();
			}
		}
		
		public double OriginY {
			get { return originY; }
			set {
				yrangeChanged = true;
				originY = value;
				OnSerieChanged ();
			}
		}
		
		public double StartX {
			get { return startX; }
			set {
				xrangeChanged = true;
				startX = value;
				if (startX > endX)
					endX = startX;
				OriginX = value;
				UpdateCursors ();
				OnSerieChanged ();
			}
		}
		
		public double EndX {
			get { return endX; }
			set {
				xrangeChanged = true;
				endX = value;
				if (endX < startX)
					startX = endX;
				UpdateCursors ();
				OnSerieChanged ();
			}
		}
		
		public double StartY {
			get { return startY; }
			set {
				yrangeChanged = true;
				startY = value;
				if (startY > endY)
					endY = startY;
				OriginY = value;
				UpdateCursors ();
				OnSerieChanged ();
			}
		}
		
		public double EndY {
			get { return endY; }
			set {
				yrangeChanged = true;
				endY = value;
				if (endY < startY)
					startY = endY;
				UpdateCursors ();
				OnSerieChanged ();
			}
		}
		
		void FixOrigins ()
		{
			if (originX < startX)
				originX = startX;
			else if (originX > endX)
				originX = endX;
			if (originY < startY)
				originY = startY;
			else if (originY > endY)
				originY = endY;
		}
		
		public void Reset ()
		{
			ArrayList list = (ArrayList) series.Clone ();
			foreach (Serie s in list)
				RemoveSerie (s);

			axis.Clear ();
		}
		
		public void AddAxis (Axis ax, AxisPosition position)
		{
			ax.Owner = this;
			ax.Position = position;
			axis.Add (ax);
			QueueDraw ();
		}
		
		public void AddSerie (Serie serie)
		{
			serie.Owner = this;
			series.Add (serie);
			OnSerieChanged ();
		}
		
		public void RemoveSerie (Serie serie)
		{
			series.Remove (serie);
			serie.Owner = null;
			OnSerieChanged ();
		}
		
		public void AddCursor (ChartCursor cursor, AxisDimension dimension)
		{
			cursor.Dimension = dimension;
			cursor.ValueChanged += new EventHandler (OnCursorChanged);
			cursor.LayoutChanged += new EventHandler (OnCursorChanged);
			cursors.Add (cursor);
			xrangeChanged = yrangeChanged = true;
			QueueDraw ();
		}
		
		public void RemoveCursor (ChartCursor cursor)
		{
			cursor.ValueChanged -= new EventHandler (OnCursorChanged);
			cursor.LayoutChanged -= new EventHandler (OnCursorChanged);
			cursors.Remove (cursor);
			QueueDraw ();
		}
		
		public void SetAutoScale (AxisDimension ad, bool autoStart, bool autoEnd)
		{
			if (ad == AxisDimension.X) {
				autoStartX = autoStart;
				autoEndX = autoEnd;
			} else {
				autoStartY = autoStart;
				autoEndY = autoEnd;
			}
		}
		
		void UpdateCursors ()
		{
			foreach (ChartCursor c in cursors) {
				if (c.Value < GetStart (c.Dimension)) c.Value = GetStart (c.Dimension);
				else if (c.Value > GetEnd (c.Dimension)) c.Value = GetEnd (c.Dimension);
			}
		}
		
		void OnCursorChanged (object sender, EventArgs args)
		{
			ChartCursor c = (ChartCursor) sender;
			if (c.Value < GetStart (c.Dimension)) c.Value = GetStart (c.Dimension);
			else if (c.Value > GetEnd (c.Dimension)) c.Value = GetEnd (c.Dimension);
			QueueDraw ();
		}
		
		internal void OnSerieChanged ()
		{
			xrangeChanged = true;
			yrangeChanged = true;
			QueueDraw ();
		}
		
		public void Clear ()
		{
			foreach (Serie s in series)
				s.Clear ();
			OnSerieChanged ();
		}
		
		double GetOrigin (AxisDimension ad)
		{
			if (ad == AxisDimension.X)
				return OriginX;
			else
				return OriginY;
		}
		
		double GetStart (AxisDimension ad)
		{
			if (ad == AxisDimension.X)
				return startX;
			else
				return startY;
		}
		
		double GetEnd (AxisDimension ad)
		{
			if (ad == AxisDimension.X)
				return endX;
			else
				return endY;
		}
		
		double GetAreaSize (AxisDimension ad)
		{
			if (ad == AxisDimension.X)
				return width;
			else
				return height;
		}
		
		double GetMinTickStep (AxisDimension ad)
		{
			return (((double) minTickStep) * (GetEnd (ad) - GetStart (ad))) / (double) GetAreaSize (ad);
		}
		
		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			double rwidth = Bounds.Width, rheight = Bounds.Height;
			
			if (autoStartY || autoEndY) {
				double nstartY = double.MaxValue;
				double nendY = double.MinValue;
				GetValueRange (AxisDimension.Y, out nstartY, out nendY);
				
				if (!autoStartY) nstartY = startY;
				if (!autoEndY) nendY = endY;
				if (nendY < nstartY) nendY = nstartY;
				
				if (nstartY != startY || nendY != endY) {
					yrangeChanged = true;
					startY = nstartY;
					endY = nendY;
				}
			}
			
			if (autoStartX || autoEndX) {
				double nstartX = double.MaxValue;
				double nendX = double.MinValue;
				GetValueRange (AxisDimension.X, out nstartX, out nendX);
				
				if (!autoStartX) nstartX = startX;
				if (!autoEndX) nendX = endX;
				if (nendX < nstartX) nendX = nstartX;
				
				if (nstartX != startX || nendX != endX) {
					xrangeChanged = true;
					startX = nstartX;
					endX = nendX;
				}
			}
			
			if (yrangeChanged) {
				FixOrigins ();
				double right = rwidth - 2 - AreaBorderWidth;
				left = AreaBorderWidth;
				left += MeasureAxisSize (ctx, AxisPosition.Left) + 1;
				right -= MeasureAxisSize (ctx, AxisPosition.Right) + 1;
				yrangeChanged = false;
				width = right - left + 1;
				if (width <= 0) width = 1;
			}
			
			if (xrangeChanged) {
				FixOrigins ();
				double bottom = rheight - 2 - AreaBorderWidth;
				top = AreaBorderWidth;
				bottom -= MeasureAxisSize (ctx, AxisPosition.Bottom);
				top += MeasureAxisSize (ctx, AxisPosition.Top);
				
				// Make room for cursor handles
				foreach (ChartCursor cursor in cursors) {
					if (cursor.Dimension == AxisDimension.X && top - AreaBorderWidth < cursor.HandleSize)
						top = cursor.HandleSize + AreaBorderWidth;
				}
				
				xrangeChanged = false;
				height = bottom - top + 1;
				if (height <= 0) height = 1;
			}
			
			if (AutoScaleMargin != 0 && height > 0) {
				double margin = (double)AutoScaleMargin * (endY - startY) / (double) height;
				if (autoStartY) startY -= margin;
				if (autoEndY) endY += margin;
			}

//			Console.WriteLine ("L:" + left + " T:" + top + " W:" + width + " H:" + height);
			
			// Draw the background

			if (backgroundDisplay == BackgroundDisplay.Gradient) {
				ctx.Rectangle (left - 1, top - 1, width + 2, height + 2);
				Gradient pat = new LinearGradient (left - 1, top - 1, left - 1, height + 2);
				pat.AddColorStop (0, backroundColor);
				Color endc = new Color (1,1,1);
				pat.AddColorStop (1, endc);
				ctx.Pattern = pat;
				ctx.Fill ();
			} else {
				ctx.Rectangle (left - 1, top - 1, width + 2, height + 2);
				ctx.SetColor (backroundColor);
				ctx.Fill ();
			}
//			win.DrawRectangle (Style.WhiteGC, true, left - 1, top - 1, width + 2, height + 2);
			ctx.SetColor (Colors.Black);
			ctx.Rectangle (left - AreaBorderWidth, top - AreaBorderWidth, width + AreaBorderWidth*2, height + AreaBorderWidth*2);
			ctx.Stroke ();
			
			// Draw selected area
			
			if (enableSelection) {
				int sx, sy, ex, ey;
				GetPoint (selectionStart.Value, selectionStart.Value, out sx, out sy);
				GetPoint (selectionEnd.Value, selectionEnd.Value, out ex, out ey);
				if (sx > ex) {
					int tmp = sx; sx = ex; ex = tmp;
				}
				ctx.SetColor (new Color (225d/255d, 225d/255d, 225d/255d));
				ctx.Rectangle (sx, top, ex - sx, height + 1);
				ctx.Fill ();
			}
			
			// Draw axes
			
			foreach (Axis ax in axis)
				DrawAxis (ctx, ax);
			
			// Draw values
			foreach (Serie serie in series)
				if (serie.Visible)
					DrawSerie (ctx, serie);

			// Draw cursors
			foreach (ChartCursor cursor in cursors)
				DrawCursor (ctx, cursor);
			
			// Draw cursor labels
			foreach (ChartCursor cursor in cursors)
				if (cursor.ShowValueLabel)
					DrawCursorLabel (ctx, cursor);
			
			((IDisposable)ctx).Dispose ();
		}
		
		void GetValueRange (AxisDimension ad, out double min, out double max)
		{
			min = double.MaxValue;
			max = double.MinValue;
			
			foreach (Serie serie in series) {
				if (!serie.HasData || !serie.Visible)
					continue;
				
				double lmin, lmax;
				serie.GetRange (ad, out lmin, out lmax);
				if (lmin < min) min = lmin;
				if (lmax > max) max = lmax;
			}
		}
		
		void DrawAxis (Context ctx, Axis ax)
		{
			ctx.SetLineWidth (1);
			double minStep = GetMinTickStep (ax.Dimension);
			
			TickEnumerator enumSmall = ax.GetTickEnumerator (minStep);
			if (enumSmall == null)
				return;
				
			TickEnumerator enumBig = ax.GetTickEnumerator (minStep * 2);
			
			if (enumBig == null) {
				DrawTicks (ctx, enumSmall, ax.Position, ax.Dimension, ax.TickSize, ax.ShowLabels);
			} else {
				DrawTicks (ctx, enumSmall, ax.Position, ax.Dimension, ax.TickSize / 2, false);
				DrawTicks (ctx, enumBig, ax.Position, ax.Dimension, ax.TickSize, ax.ShowLabels);
			}
		}
		
		void DrawTicks (Context ctx, TickEnumerator e, AxisPosition pos, AxisDimension ad, int tickSize, bool showLabels)
		{
			double rheight = Bounds.Height;
			
			TextLayout layout = null;
			
			if (showLabels) {
				layout = new TextLayout ();
				layout.Font = chartFont;
			}
			
			bool isX = pos == AxisPosition.Top || pos == AxisPosition.Bottom;
			bool isTop = pos == AxisPosition.Top || pos == AxisPosition.Right;
			
			double start = GetStart (ad);
			double end = GetEnd (ad);
			
			e.Init (GetOrigin (ad));
			
			while (e.CurrentValue > start)
				e.MovePrevious ();
			
			double lastPosLabel;
			double lastPos;
			double lastTw = 0;
			
			if (isX) {
				lastPosLabel = reverseXAxis ? left + width + MinLabelGapX : left - MinLabelGapX;
				lastPos = left - minTickStep*2;
			}
			else {
				lastPosLabel = reverseYAxis ? top - MinLabelGapY : rheight + MinLabelGapY;
				lastPos = top + height + minTickStep*2;
			}
			
			for ( ; e.CurrentValue <= end; e.MoveNext ())
			{
				double px, py;
				double tw = 0, th = 0;
				int tick = tickSize;
				
				GetPoint (e.CurrentValue, e.CurrentValue, out px, out py);
				
				if (showLabels) {
					layout.Text = e.CurrentLabel;
					var ts = layout.GetSize ();
					tw = ts.Width;
					th = ts.Height;
				}

				if (isX) {
					if (Math.Abs ((long)px - (long)lastPos) < minTickStep || px < left || px > left + width)
						continue;
					lastPos = px;
					
					bool labelFits = false;
					if ((Math.Abs (px - lastPosLabel) - (tw/2) - (lastTw/2)) >= MinLabelGapX) {
						lastPosLabel = px;
						lastTw = tw;
						labelFits = true;
					}
					
					if (isTop) {
						if (showLabels) {
							if (labelFits)
								ctx.DrawTextLayout (layout, px - (tw/2), top - AreaBorderWidth - th);
							else
								tick = tick / 2;
						}
						ctx.MoveTo (px, top);
						ctx.LineTo (px, top + tick);
						ctx.Stroke ();
					}
					else {
						if (showLabels) {
							if (labelFits)
								ctx.DrawTextLayout (layout, px - (tw/2), top + height + AreaBorderWidth);
							else
								tick = tick / 2;
						}
						ctx.MoveTo (px, top + height);
						ctx.LineTo (px, top + height - tick);
						ctx.Stroke ();
					}
				}
				else {
					if (Math.Abs ((long)lastPos - (long)py) < minTickStep || py < top || py > top + height)
						continue;
					lastPos = py;
					
					bool labelFits = false;
					if ((Math.Abs (py - lastPosLabel) - (th/2) - (lastTw/2)) >= MinLabelGapY) {
						lastPosLabel = py;
						lastTw = th;
						labelFits = true;
					}
					
					if (isTop) {
						if (showLabels) {
							if (labelFits)
								ctx.DrawTextLayout (layout, left + width + AreaBorderWidth + 1, py - (th/2));
							else
								tick = tick / 2;
						}
						ctx.MoveTo (left + width, py);
						ctx.LineTo (left + width - tick, py);
						ctx.Stroke ();
					}
					else {
						if (showLabels) {
							if (labelFits)
								ctx.DrawTextLayout (layout, left - AreaBorderWidth - tw - 1, py - (th/2));
							else
								tick = tick / 2;
						}
						ctx.MoveTo (left, py);
						ctx.LineTo (left + tick, py);
						ctx.Stroke ();
					}
				}
			}
		}
		
		double MeasureAxisSize (Context ctx, AxisPosition pos)
		{
			double max = 0;
			foreach (Axis ax in axis)
				if (ax.Position == pos && ax.ShowLabels) {
					double nmax = MeasureAxisSize (ctx, ax);
					if (nmax > max) max = nmax;
				}
			return max;
		}
		
		double MeasureAxisSize (Context ctx, Axis ax)
		{
			double minStep = GetMinTickStep (ax.Dimension);
			
			TickEnumerator enumSmall = ax.GetTickEnumerator (minStep);
			if (enumSmall == null)
				return 0;
				
			TickEnumerator enumBig = ax.GetTickEnumerator (minStep * 2);
			
			if (enumBig == null)
				return MeasureTicksSize (ctx, enumSmall, ax.Dimension);
			else
				return MeasureTicksSize (ctx, enumBig, ax.Dimension);
		}
		
		double MeasureTicksSize (Context ctx, TickEnumerator e, AxisDimension ad)
		{
			double max = 0;
			TextLayout layout = new TextLayout ();
			layout.Font = chartFont;
			
			double start = GetStart (ad);
			double end = GetEnd (ad);
			
			e.Init (GetOrigin (ad));
			
			while (e.CurrentValue > start)
				e.MovePrevious ();
			
			for ( ; e.CurrentValue <= end; e.MoveNext ())
			{
				layout.Text = e.CurrentLabel;
				Size ts = layout.GetSize ();
				
				if (ad == AxisDimension.X) {
					if (ts.Height > max)
						max = ts.Height;
				} else {
					if (ts.Width > max)
						max = ts.Width;
				}
			}
			return max;
		}
		
		void DrawSerie (Context ctx, Serie serie)
		{
			ctx.NewPath ();
			ctx.Rectangle (left, top, width + 1, height + 1);
			ctx.Clip ();
			
			ctx.NewPath ();
			ctx.SetColor (serie.Color);
			ctx.SetLineWidth (serie.LineWidth);
			
			bool first = true;
			bool blockMode = serie.DisplayMode == DisplayMode.BlockLine;
			
			double lastY = 0;
			
			foreach (Data d in serie.GetData (startX, endX)) {
				double x, y;
				GetPoint (d.X, d.Y, out x, out y);
				if (first) {
					ctx.MoveTo (x, y);
					lastY = y;
					first = false;
				} else {
					if (blockMode) {
						if (lastY != y)
							ctx.LineTo (x, lastY);
						ctx.LineTo (x, y);
					} else
						ctx.LineTo (x, y);
				}
				lastY = y;
			}
			
			ctx.Stroke ();
		}
		
		void DrawCursor (Context ctx, ChartCursor cursor)
		{
			ctx.SetColor (cursor.Color);
			
			double x, y;
			GetPoint (cursor.Value, cursor.Value, out x, out y);
				
			if (cursor.Dimension == AxisDimension.X) {
				double cy = top - AreaBorderWidth - 1;
				ctx.MoveTo (x, cy);
				ctx.LineTo (x + (cursor.HandleSize/2), cy - cursor.HandleSize + 1);
				ctx.LineTo (x - (cursor.HandleSize/2), cy - cursor.HandleSize + 1);
				ctx.ClosePath ();
				if (activeCursor == cursor)
					ctx.FillPreserve ();
				ctx.Stroke ();
				ctx.MoveTo (x, top);
				ctx.RelLineTo (0, height);
				ctx.Stroke ();
			} else {
				throw new NotSupportedException ();
			}
		}
		
		void DrawCursorLabel (Context ctx, ChartCursor cursor)
		{
			ctx.SetColor (cursor.Color);
			
			int x, y;
			GetPoint (cursor.Value, cursor.Value, out x, out y);

			if (cursor.Dimension == AxisDimension.X) {
			
				string text;
				
				if (cursor.LabelAxis != null) {
					double minStep = GetMinTickStep (cursor.Dimension);
					TickEnumerator tenum = cursor.LabelAxis.GetTickEnumerator (minStep);
					tenum.Init (cursor.Value);
					text = tenum.CurrentLabel;
				} else {
					text = GetValueLabel (cursor.Dimension, cursor.Value);
				}
				
				if (text != null && text.Length > 0) {
					TextLayout layout = new TextLayout ();
					layout.Font = chartFont;
					layout.Text = text;
					
					Size ts = layout.GetSize ();
					double tl = x - ts.Width/2;
					double tt = top + 4;
					if (tl + ts.Width + 2 >= left + width) tl = left + width - ts.Width - 1;
					if (tl < left + 1) tl = left + 1;
					ctx.SetColor (Colors.White);
					ctx.Rectangle (tl - 1, tt - 1, ts.Width + 2, ts.Height + 2);
					ctx.Fill ();
					ctx.Rectangle (tl - 2, tt - 2, ts.Width + 3, ts.Height + 3);
					ctx.SetColor (Colors.Black);
					ctx.DrawTextLayout (layout, tl, tt);
				}
			} else {
				throw new NotSupportedException ();
			}
		}
		
		void GetPoint (double wx, double wy, out int x, out int y)
		{
			double dx, dy;
			GetPoint (wx, wy, out dx, out dy);
			x = (int) dx;
			y = (int) dy;
		}
		
		void GetPoint (double wx, double wy, out double x, out double y)
		{
			unchecked {
				if (reverseXAxis)
					x = left + width - (((wx - startX) * ((double) width)) / (endX - startX));
				else
					x = left + (((wx - startX) * ((double) width)) / (endX - startX));

				if (reverseYAxis)
					y = top + ((wy - startY) * ((double) height) / (endY - startY));
				else
					y = top + height - ((wy - startY) * ((double) height) / (endY - startY));
			}
		}
		
		void GetValue (int x, int y, out double wx, out double wy)
		{
			unchecked {
				if (reverseXAxis)
					wx = startX + ((double) (left + width - 1 - x)) * (endX - startX) / (double) width;
				else
					wx = startX + ((double) (x - left)) * (endX - startX) / (double) width;

				if (reverseYAxis)
					wy = startY + ((double) (top + y)) * (endY - startY) / (double) height;
				else
					wy = startY + ((double) (top + height - y - 1)) * (endY - startY) / (double) height;
			}
		}
		
		string GetValueLabel (AxisDimension ad, double value)
		{
			foreach (Axis ax in axis)
				if (ax.Dimension == ad)
					return ax.GetValueLabel (value);
			return null;
		}
		
		internal void OnLayoutChanged ()
		{
			xrangeChanged = true;
			yrangeChanged = true;
			QueueDraw ();
		}
		
		void OnSelectionCursorChanged (object sender, EventArgs args)
		{
			if (enableSelection) {
				if (selectionStart.Value > selectionEnd.Value) {
					ChartCursor tmp = selectionStart;
					selectionStart = selectionEnd;
					selectionEnd = tmp;
				}
				OnSelectionChanged ();
			}
		}
		
		protected override void OnButtonPressed (ButtonEventArgs ev)
		{
			if (ev.Button == PointerButton.Left) {
				foreach (ChartCursor cursor in cursors) {
					int cx, cy;
					GetPoint (cursor.Value, cursor.Value, out cx, out cy);
					if (cursor.Dimension == AxisDimension.X) {
						if (Math.Abs (ev.X - cx) <= 2 || (ev.Y < top && (Math.Abs (ev.X - cx) <= cursor.HandleSize/2))) {
							activeCursor = cursor;
							draggingCursor = true;
							activeCursor.ShowValueLabel = true;
							QueueDraw ();
							break;
						}
					} else {
						// Implement
					}
				}
				
				if (enableSelection && !draggingCursor) {
					selectionStart.Visible = true;
					selectionEnd.Visible = true;
					
					double x, y;
					GetValue ((int)ev.X, (int)ev.Y, out x, out y);
					// avoid cursor swaping
					ChartCursor c1 = selectionStart;
					ChartCursor c2 = selectionEnd;
					c1.Value = x;
					c2.Value = x;
					activeCursor = selectionEnd;
					activeCursor.ShowValueLabel = true;
					draggingCursor = true;
					QueueDraw ();
				}
				
				if (draggingCursor)
					return;
			}
			base.OnButtonPressed (ev);
			return;
		}

		protected override void OnButtonReleased (ButtonEventArgs e)
		{
			if (draggingCursor) {
				draggingCursor = false;
				activeCursor.ShowValueLabel = false;
			}
			base.OnButtonReleased (e);
		}
		
		
		protected override void OnMouseMoved (MouseMovedEventArgs e)
		{
			if (draggingCursor) {
				double x, y;
				GetValue ((int)e.X, (int)e.Y, out x, out y);
				
				if (activeCursor.Dimension == AxisDimension.X) {
					if (x < startX) x = startX;
					else if (x > endX) x = endX;
					activeCursor.Value = x;
				}
				else {
					if (y < startY) y = startY;
					else if (y > endY) y = endY;
					activeCursor.Value = y;
				}
				return;
			}
			base.OnMouseMoved (e);
		}
		
		protected override void OnBoundsChanged ()
		{
			xrangeChanged = true;
			yrangeChanged = true;
			base.OnBoundsChanged ();
		}
		
		protected virtual void OnSelectionChanged ()
		{
			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}
	}
	
	public enum BackgroundDisplay
	{
		Solid,
		Gradient
	}
}
