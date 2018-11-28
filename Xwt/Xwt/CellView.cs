// 
// CellView.cs
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
using Xwt.Drawing;
using Xwt.Backends;
using Xwt.Accessibility;
using System.ComponentModel;
using System.Collections.Generic;

namespace Xwt
{
	public class CellView: XwtComponent, ICellViewFrontend
	{
		Widget container;
		bool expands;

		static CellView ()
		{
			EventHost.MapEvent (WidgetEvent.KeyPressed, typeof(CellView), "OnKeyPressed");
			EventHost.MapEvent (WidgetEvent.KeyReleased, typeof(CellView), "OnKeyReleased");
			EventHost.MapEvent (WidgetEvent.MouseEntered, typeof(CellView), "OnMouseEntered");
			EventHost.MapEvent (WidgetEvent.MouseExited, typeof(CellView), "OnMouseExited");
			EventHost.MapEvent (WidgetEvent.ButtonPressed, typeof(CellView), "OnButtonPressed");
			EventHost.MapEvent (WidgetEvent.ButtonReleased, typeof(CellView), "OnButtonReleased");
			EventHost.MapEvent (WidgetEvent.MouseMoved, typeof(CellView), "OnMouseMoved");
		}

		/// <summary>
		/// Gets the default cell view for the provided field type
		/// </summary>
		/// <returns>The default cell view.</returns>
		/// <param name="field">Field.</param>
		public static CellView GetDefaultCellView (IDataField field)
		{
			if (field.Index == -1)
				throw new InvalidOperationException ("Field must be bound to a data source");
			if (field.FieldType == typeof(bool))
				return new CheckBoxCellView ((IDataField<bool>)field);
			else if (field.FieldType == typeof(CheckBoxState))
				return new CheckBoxCellView ((IDataField<CheckBoxState>)field);
			else if (field.FieldType == typeof(Image))
				return new ImageCellView ((IDataField<Image>)field);
			return new TextCellView (field);
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new CellViewBackendHost ();
		}

		protected new CellViewBackendHost BackendHost {
			get { return (CellViewBackendHost) base.BackendHost; }
		}

		ICellViewBackend Backend {
			get { return (ICellViewBackend) base.BackendHost.Backend; }
		}

		protected class CellViewBackendHost: BackendHost<CellView,ICellViewBackend>, ICellViewEventSink
		{
			HashSet<object> enabledEvents;

			protected override void OnEnableEvent (object eventId)
			{
				if (enabledEvents == null)
					enabledEvents = new HashSet<object> ();
				enabledEvents.Add (eventId);
				if (BackendCreated)
					base.OnEnableEvent (eventId);
			}

			protected override void OnDisableEvent (object eventId)
			{
				if (enabledEvents != null)
					enabledEvents.Remove (eventId);
				if (BackendCreated)
					base.OnDisableEvent (eventId);
			}

			public void AttachBackend (ICellViewBackend backend)
			{
				SetCustomBackend (backend);
				if (enabledEvents != null) {
					foreach (var e in enabledEvents)
						Backend.EnableEvent (e);
				}
			}

			#region ICellViewEventSink implementation

			public void OnKeyPressed (KeyEventArgs args)
			{
				Parent.OnKeyPressed (args);
			}

			public void OnKeyReleased (KeyEventArgs args)
			{
				Parent.OnKeyReleased (args);
			}

			public void OnMouseEntered ()
			{
				Parent.OnMouseEntered ();
			}

			public void OnMouseExited ()
			{
				Parent.OnMouseExited ();
			}

			public void OnMouseMoved (MouseMovedEventArgs args)
			{
				Parent.OnMouseMoved (args);
			}

			public void OnButtonPressed (ButtonEventArgs args)
			{
				Parent.OnButtonPressed (args);
			}

			public void OnButtonReleased (ButtonEventArgs args)
			{
				Parent.OnButtonReleased (args);
			}

			#endregion
		}

		/// <summary>
		/// Data source object to be used to get the data with which to fill the cell
		/// </summary>
		/// <value>The data source.</value>
		protected ICellDataSource DataSource { get; private set; }

		bool visible = true;

		public IDataField<bool> VisibleField { get; set; }

		[DefaultValue (true)]
		public bool Visible {
			get { return GetValue (VisibleField, visible); }
			set { visible = value; }
		}

		public AccessibleFields AccessibleFields { get; set; }

		ICellViewEventSink ICellViewFrontend.Load (ICellDataSource dataSource)
		{
			DataSource = dataSource;
			OnDataChanged ();
			return BackendHost;
		}

		void ICellViewFrontend.Unload ()
		{
		}

		void ICellViewFrontend.AttachBackend (Widget container, ICellViewBackend backend)
		{
			this.container = container;
			BackendHost.AttachBackend (backend);
		}

		void ICellViewFrontend.DetachBackend ()
		{
		}

		public Widget ParentWidget {
			get { return container; }
		}

		protected Rectangle Bounds {
			get { return Backend.CellBounds; }
		}

		protected Rectangle BackgroundBounds {
			get { return Backend.BackgroundBounds; }
		}

		protected bool Selected {
			get { return Backend.Selected; }
		}

		protected bool HasFocus {
			get { return Backend.HasFocus; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Xwt.CellView"/> expands to fill all available horizontal space.
		/// </summary>
		/// <value><c>true</c> if the cell expands horizontally; otherwise, <c>false</c>.</value>
		public bool Expands {
			get { return expands; }
			internal set { expands = value; }
		}

		/// <summary>
		/// Gets the value of a field
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="field">Field.</param>
		/// <param name="defaultValue">Default value to be returned if the field has no value</param>
		/// <typeparam name="T">Type of the value</typeparam>
		protected T GetValue<T> (IDataField<T> field, T defaultValue = default(T))
		{
			if (DataSource != null && field != null) {
				var result = DataSource.GetValue (field);
				return result == null || result == DBNull.Value ? defaultValue : (T) result;
			}
			return defaultValue;
		}

		/// <summary>
		/// Invoked when the data source changes
		/// </summary>
		protected virtual void OnDataChanged ()
		{
		}

		EventHandler<KeyEventArgs> keyPressed;
		EventHandler<KeyEventArgs> keyReleased;
		EventHandler mouseEntered;
		EventHandler mouseExited;
		EventHandler<MouseMovedEventArgs> mouseMoved;
		EventHandler<ButtonEventArgs> buttonPressed;
		EventHandler<ButtonEventArgs> buttonReleased;

		public event EventHandler<KeyEventArgs> KeyPressed {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.KeyPressed, keyPressed);
				keyPressed += value;
			}
			remove {
				keyPressed -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.KeyPressed, keyPressed);
			}
		}

		public event EventHandler<KeyEventArgs> KeyReleased {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.KeyReleased, keyReleased);
				keyReleased += value;
			}
			remove {
				keyReleased -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.KeyReleased, keyReleased);
			}
		}

		public event EventHandler MouseEntered {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseEntered, mouseEntered);
				mouseEntered += value;
			}
			remove {
				mouseEntered -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseEntered, mouseEntered);
			}
		}

		public event EventHandler MouseExited {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseExited, mouseExited);
				mouseExited += value;
			}
			remove {
				mouseExited -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseExited, mouseExited);
			}
		}

		public event EventHandler<ButtonEventArgs> ButtonPressed {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.ButtonPressed, buttonPressed);
				buttonPressed += value;
			}
			remove {
				buttonPressed -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.ButtonPressed, buttonPressed);
			}
		}

		public event EventHandler<ButtonEventArgs> ButtonReleased {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.ButtonReleased, buttonReleased);
				buttonReleased += value;
			}
			remove {
				buttonReleased -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.ButtonReleased, buttonReleased);
			}
		}

		public event EventHandler<MouseMovedEventArgs> MouseMoved {
			add {
				BackendHost.OnBeforeEventAdd (WidgetEvent.MouseMoved, mouseMoved);
				mouseMoved += value;
			}
			remove {
				mouseMoved -= value;
				BackendHost.OnAfterEventRemove (WidgetEvent.MouseMoved, mouseMoved);
			}
		}

		internal protected virtual void OnKeyPressed (KeyEventArgs args)
		{
			if (keyPressed != null)
				keyPressed (this, args);
		}

		internal protected virtual void OnKeyReleased (KeyEventArgs args)
		{
			if (keyReleased != null)
				keyReleased (this, args);
		}

		internal protected virtual void OnMouseEntered ()
		{
			if (mouseEntered != null)
				mouseEntered (this, EventArgs.Empty);
		}

		internal protected virtual void OnMouseExited ()
		{
			if (mouseExited != null)
				mouseExited (this, EventArgs.Empty);
		}

		internal protected virtual void OnMouseMoved (MouseMovedEventArgs args)
		{
			if (mouseMoved != null)
				mouseMoved (this, args);
		}

		internal protected virtual void OnButtonPressed (ButtonEventArgs args)
		{
			if (buttonPressed != null)
				buttonPressed (this, args);
		}

		internal protected virtual void OnButtonReleased (ButtonEventArgs args)
		{
			if (buttonReleased != null)
				buttonReleased (this, args);
		}
	}
}
