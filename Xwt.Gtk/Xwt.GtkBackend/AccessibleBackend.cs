//
// AccessibleBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corporation
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
using Xwt.Accessibility;
using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class AccessibleBackend : IAccessibleBackend
	{
		WidgetBackend backend;
		IAccessibleEventSink eventSink;
		ApplicationContext context;

		public AccessibleBackend ()
		{
		}

		public void Initialize (IAccessibleEventSink eventSink)
		{
			this.eventSink = eventSink;
		}

		public void InitializeBackend (object frontend, ApplicationContext context)
		{
			this.context = context;
			var parent = (XwtComponent)frontend;
			backend = parent.GetBackend () as WidgetBackend;
		}

		public Rectangle Bounds {
			get {
				int x = 0, y = 0, w = 0, h = 0;
				(backend.Widget.Accessible as Atk.Component)?.GetExtents (out x, out y, out w, out h, Atk.CoordType.Screen);
				return new Rectangle (x, y, w, h);
			}
			set {
				(backend.Widget.Accessible as Atk.Component)?.SetExtents ((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height, Atk.CoordType.Screen);
			}
		}

		public string Description {
			get {
				return backend.Widget.Accessible.Description;
			}
			set {
				backend.Widget.Accessible.Description = value;
			}
		}

		public string Label {
			get {
				return backend.Widget.Accessible.Name;
			}
			set {
				backend.Widget.Accessible.Name = value;
			}
		}

		public Role Role {
			get {
				return backend.Widget.Accessible.Role.ToXwtRole ();
			}

			set {
				backend.Widget.Accessible.Role = value.ToAtkRole ();
			}
		}

		public string RoleDescription { get; set; }

		public string Title { get; set; }

		public string Value {
			get {
				if (backend.Widget.Accessible is Atk.Value) {
					GLib.Value val = GLib.Value.Empty;
					(backend.Widget.Accessible as Atk.Value)?.GetCurrentValue (ref val);
					return val.Val.ToString ();
				}
				if (backend.Widget.Accessible is Atk.Text) {
					var atkText = (backend.Widget.Accessible as Atk.Text);
					return atkText?.GetText (0, atkText.CharacterCount - 1);
				}
				return null;
			}
			set {
				if (backend.Widget.Accessible is Atk.Value) {
					GLib.Value val = GLib.Value.Empty;
					(backend.Widget.Accessible as Atk.Value)?.SetCurrentValue (new GLib.Value (value));
				} else if (backend.Widget.Accessible is Atk.EditableText) {
					var atkText = (backend.Widget.Accessible as Atk.EditableText);
					atkText.TextContents = value;
				}
			}
		}

		public bool IsAccessible {
			get {
				return backend.Widget.Accessible?.Role != Atk.Role.Invalid;
			}
			set {
				// TODO
				//if (backend.Widget.Accessible != null)
				//	return backend.Widget.Accessible.Role = Atk.Role.Invalid;
			}
		}

		public void DisableEvent (object eventId)
		{
			throw new NotImplementedException ();
		}

		public void EnableEvent (object eventId)
		{
			throw new NotImplementedException ();
		}
	}
}
