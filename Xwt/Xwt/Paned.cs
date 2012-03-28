// 
// Paned.cs
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
using System.Windows.Markup;

namespace Xwt
{
	public class Paned: Widget
	{
		Orientation direction;
		Panel panel1;
		Panel panel2;
		EventHandler positionChanged;
		
		protected new class EventSink: Widget.EventSink, IContainerEventSink<Panel>, IPanedEventSink
		{
			public void ChildChanged (Panel child, string hint)
			{
				((Paned)Parent).OnChildChanged (child, hint);
			}
			
			public void ChildReplaced (Panel child, Widget oldWidget, Widget newWidget)
			{
				((Paned)Parent).OnReplaceChild (child, oldWidget, newWidget);
			}
			
			public void OnPositionChanged ()
			{
				((Paned)Parent).NotifyPositionChanged ();
			}
		}
		
		static Paned ()
		{
			MapEvent (PanedEvent.PositionChanged, typeof(Paned), "OnPositionChanged");
		}
		
		internal Paned (Orientation direction)
		{
			this.direction = direction;
			panel1 = new Panel ((EventSink)WidgetEventSink, 1);
			panel2 = new Panel ((EventSink)WidgetEventSink, 2);
		}
		
		protected override IBackend OnCreateBackend ()
		{
			IPanedBackend b = (IPanedBackend) base.OnCreateBackend ();
			
			// We always want to listen this event because we use it
			// to reallocate the children
			if (!Application.EngineBackend.HandlesSizeNegotiation)
				b.EnableEvent (PanedEvent.PositionChanged);
			
			return b;
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IPanedBackend Backend {
			get { return (IPanedBackend) base.Backend; }
		}
		
		/// <summary>
		/// Left or top panel
		/// </summary>
		public Panel Panel1 {
			get { return panel1; }
		}

		/// <summary>
		/// Right or bottom panel
		/// </summary>
		public Panel Panel2 {
			get { return panel2; }
		}

		/// <summary>
		/// Gets or sets the position of the panel separator
		/// </summary>
		/// <value>
		/// The position.
		/// </value>
		public double Position {
			get { return Backend.Position; }
			set { Backend.Position = value; }
		}

		/// <summary>
		/// Gets or sets the position of the panel separator as a fraction available size
		/// </summary>
		/// <value>
		/// The position.
		/// </value>
		public double PositionFraction {
			get {
				return Backend.Position / ((direction == Orientation.Horizontal) ? ScreenBounds.Width : ScreenBounds.Height);
			}
			set { Backend.Position = ((direction == Orientation.Horizontal) ? ScreenBounds.Width : ScreenBounds.Height) * value; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.Initialize (direction);
		}

		void OnReplaceChild (Panel panel, Widget oldChild, Widget newChild)
		{
			if (oldChild != null) {
				Backend.RemovePanel (panel.NumPanel);
				UnregisterChild (oldChild);
			}
			if (newChild != null) {
				RegisterChild (newChild);
				Backend.SetPanel (panel.NumPanel, (IWidgetBackend)GetBackend (newChild), panel.Resize);
				UpdatePanel (panel);
			}
		}
		
		/// <summary>
		/// Removes a child widget
		/// </summary>
		/// <param name='child'>
		/// A widget bound to one of the panels
		/// </param>
		public void Remove (Widget child)
		{
			if (panel1.Content == child)
				panel1.Content = null;
			else if (panel2.Content == child)
				panel2.Content = null;
		}
		
		void OnChildChanged (Panel panel, object hint)
		{
			UpdatePanel (panel);
		}

		void UpdatePanel (Panel panel)
		{
			double minSize;
			if (direction == Orientation.Horizontal)
				minSize = ((IWidgetSurface)panel.Content).GetPreferredWidth ().MinSize;
			else
				minSize = ((IWidgetSurface)panel.Content).GetPreferredHeightForWidth (Backend.Size.Width - Backend.GetDecorationSize ().Width).MinSize;
			if (panel.Shrink)
				minSize = 0;
			Backend.UpdatePanel (panel.NumPanel, panel.Resize, minSize);
		}
		
		void NotifyPositionChanged ()
		{
			if (!Application.EngineBackend.HandlesSizeNegotiation) {
				if (panel1.Content != null)
					((IWidgetSurface)panel1.Content).Reallocate ();
				if (panel2.Content != null)
					((IWidgetSurface)panel2.Content).Reallocate ();
			}
			OnPositionChanged ();
		}
		
		protected virtual void OnPositionChanged ()
		{
			if (positionChanged != null)
				positionChanged (this, EventArgs.Empty);
		}
		
		public event EventHandler PositionChanged {
			add {
				OnBeforeEventAdd (PanedEvent.PositionChanged, positionChanged);
				positionChanged += value;
			}
			remove {
				positionChanged -= value;
				OnAfterEventRemove (PanedEvent.PositionChanged, positionChanged);
			}
		}

		Panel GetSinglePanel ()
		{
			if (panel1.Content != null && panel2.Content == null)
				return panel1;
			if (panel2.Content != null && panel1.Content == null)
				return panel2;
			return null;
		}

		protected override WidgetSize OnGetPreferredWidth ()
		{
			if (Application.EngineBackend.HandlesSizeNegotiation)
				return base.OnGetPreferredWidth ();

			if (panel1.Content == null && panel2.Content == null)
				return new WidgetSize (0);

			Panel singlePanel = GetSinglePanel ();
			if (singlePanel != null) {
				var ps = ((IWidgetSurface)singlePanel.Content).GetPreferredWidth ();
				if (singlePanel.Shrink)
					ps.MinSize = 0;
				return ps + Backend.GetDecorationSize ().Width;
			}

			IWidgetSurface w1 = Panel1.Content;
			IWidgetSurface w2 = Panel2.Content;

			WidgetSize s = new WidgetSize (0);
			if (direction == Orientation.Horizontal) {
				s = w1.GetPreferredWidth ();
				if (panel1.Shrink)
					s.MinSize = 0;
				if (Position != 0)
					s.NaturalSize = Math.Max (s.NaturalSize, Position);

				var s2 = w2.GetPreferredWidth ();
				if (panel2.Shrink)
					s2.MinSize = 0;
				s += s2;
			}
			else {
				if (panel1.Content != null)
					s = w1.GetPreferredWidth ();
				if (panel2.Content != null) {
					var s2 = w2.GetPreferredWidth ();
					s = new WidgetSize (Math.Max (s.MinSize, s2.MinSize), Math.Max (s.NaturalSize, s2.NaturalSize));
				}
			}
			s += new WidgetSize (Backend.GetDecorationSize ().Width);
			return s;
		}

		protected override WidgetSize OnGetPreferredHeight ()
		{
			if (Application.EngineBackend.HandlesSizeNegotiation)
				return base.OnGetPreferredHeight ();

			if (panel1.Content == null && panel2.Content == null)
				return new WidgetSize (0);

			Panel singlePanel = GetSinglePanel ();
			if (singlePanel != null) {
				var ps = ((IWidgetSurface)singlePanel.Content).GetPreferredHeight ();
				if (singlePanel.Shrink)
					ps.MinSize = 0;
				return ps + Backend.GetDecorationSize ().Height;
			}

			IWidgetSurface w1 = Panel1.Content;
			IWidgetSurface w2 = Panel2.Content;

			WidgetSize s = new WidgetSize (0);
			if (direction == Orientation.Vertical) {
				s = w1.GetPreferredHeight ();
				if (panel1.Shrink)
					s.MinSize = 0;
				if (Position != 0)
					s.NaturalSize = Math.Max (s.NaturalSize, Position);

				var s2 = w2.GetPreferredHeight ();
				if (panel2.Shrink)
					s2.MinSize = 0;
				s += s2;
			}
			else {
				if (panel1.Content != null)
					s = w1.GetPreferredHeight ();
				if (panel2.Content != null) {
					var s2 = w2.GetPreferredHeight ();
					s = new WidgetSize (Math.Max (s.MinSize, s2.MinSize), Math.Max (s.NaturalSize, s2.NaturalSize));
				}
			}
			s += new WidgetSize (Backend.GetDecorationSize ().Height);
			return s;
		}

		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			if (Application.EngineBackend.HandlesSizeNegotiation)
				return base.OnGetPreferredHeightForWidth (width);

			if (direction == Orientation.Vertical) {
				WidgetSize s = new WidgetSize (Backend.GetDecorationSize ().Height);
				if (panel1.Content != null)
					s += ((IWidgetSurface)panel1.Content).GetPreferredHeightForWidth (width);
				if (panel2.Content != null)
					s += ((IWidgetSurface)panel2.Content).GetPreferredHeightForWidth (width);
				return s;
			}
			else {
				double s1, s2;
				Backend.GetPanelSizes (width, out s1, out s2);
				WidgetSize s = new WidgetSize (0);
				if (panel1.Content != null && s1 > 0)
					s = ((IWidgetSurface)panel1.Content).GetPreferredHeightForWidth (s1);
				if (panel2.Content != null && s2 > 0)
					s = s.UnionWith (((IWidgetSurface)panel2.Content).GetPreferredHeightForWidth (s2));
				return s + Backend.GetDecorationSize ().Height;
			}
		}

		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			if (Application.EngineBackend.HandlesSizeNegotiation)
				return base.OnGetPreferredWidthForHeight (height);

			if (direction == Orientation.Horizontal) {
				WidgetSize s = new WidgetSize (Backend.GetDecorationSize ().Width);
				if (panel1.Content != null)
					s += ((IWidgetSurface)panel1.Content).GetPreferredWidthForHeight (height);
				if (panel2.Content != null)
					s += ((IWidgetSurface)panel2.Content).GetPreferredWidthForHeight (height);
				return s;
			}
			else {
				double s1, s2;
				Backend.GetPanelSizes (height, out s1, out s2);
				WidgetSize s = new WidgetSize (0);
				if (panel1.Content != null && s1 > 0)
					s = ((IWidgetSurface)panel1.Content).GetPreferredWidthForHeight (s1);
				if (panel2.Content != null && s2 > 0)
					s = s.UnionWith (((IWidgetSurface)panel2.Content).GetPreferredWidthForHeight (s2));
				return s + Backend.GetDecorationSize ().Width;
			}
		}
	}
	
	[ContentProperty("Child")]
	public class Panel
	{
		IContainerEventSink<Panel> parent;
		bool resize;
		bool shrink;
		int numPanel;
		Widget child;
		
		internal Panel (IContainerEventSink<Panel> parent, int numPanel)
		{
			this.parent = parent;
			this.numPanel = numPanel;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this panel should be resized when the Paned container is resized.
		/// </summary>
		/// <value>
		/// <c>true</c> if the panel has to be resized; otherwise, <c>false</c>.
		/// </value>
		public bool Resize {
			get {
				return this.resize;
			}
			set {
				resize = value;
				parent.ChildChanged (this, "Resize");
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this panel can be made smaller than its min size
		/// </summary>
		/// <value>
		/// <c>true</c> if the panel has to be shrinked; otherwise, <c>false</c>.
		/// </value>
		public bool Shrink {
			get {
				return this.shrink;
			}
			set {
				shrink = value;
				parent.ChildChanged (this, "Shrink");
			}
		}

		/// <summary>
		/// Gets or sets the content of the panel
		/// </summary>
		/// <value>
		/// The content.
		/// </value>
		public Widget Content {
			get {
				return child;
			}
			set {
				var old = child;
				child = value;
				parent.ChildReplaced (this, old, value);
			}
		}
		
		internal int NumPanel {
			get {
				return this.numPanel;
			}
			set {
				numPanel = value;
			}
		}
	}
}

