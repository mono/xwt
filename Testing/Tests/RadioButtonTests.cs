//
// RadioButtonTests.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
using NUnit.Framework;

namespace Xwt
{
	public class RadioButtonTests: WidgetTests
	{
		public override Widget CreateWidget ()
		{
			return new RadioButton ();
		}

		[Test]
		public void DefaultValues ()
		{
			var r = new RadioButton ();
			Assert.IsTrue (r.Active);
			Assert.NotNull (r.Group);
		}

		[Test]
		public void ActiveProperty ()
		{
			var r = new RadioButton ();
			Assert.IsTrue (r.Active);
			r.Active = false;
			Assert.IsFalse (r.Active);
			r.Active = true;
			Assert.IsTrue (r.Active);
		}

		[Test]
		public void DefaultGroupSelection ()
		{
			// The first item added to a group is active by default
			// Adding a radio to the group, doesn't change the active radio
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();

			Assert.IsTrue (r1.Active);
			Assert.IsTrue (r2.Active);
			Assert.IsTrue (r3.Active);

			r2.Group = r3.Group = r1.Group;

			Assert.IsTrue (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);
		}
		
		[Test]
		public void GroupSwitching ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;
			Assert.IsTrue (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);

			r2.Active = true;
			Assert.IsFalse (r1.Active);
			Assert.IsTrue (r2.Active);
			Assert.IsFalse (r3.Active);
			
			r3.Active = true;
			Assert.IsFalse (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsTrue (r3.Active);
			
			r1.Active = true;
			Assert.IsTrue (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);
		}
		
		[Test]
		public void AllRadiosDisabled ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;
			Assert.IsTrue (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);

			r1.Active = false;
			Assert.IsFalse (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);
		}
		
		[Test]
		public void ToggleEvent ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;

			int changed1 = 0;
			int changed2 = 0;
			int ev = 1;

			r1.ActiveChanged += delegate {
				changed1 = ev++;
			};

			r2.ActiveChanged += delegate {
				changed2 = ev++;
			};

			r2.Active = true;

			Assert.IsFalse (r1.Active);
			Assert.IsTrue (r2.Active);
			Assert.AreEqual (1, changed1);
			Assert.AreEqual (2, changed2);
		}
		
		[Test]
		public void GroupActiveRadioButton ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;
			r2.Active = true;
			Assert.IsFalse (r1.Active);
			Assert.IsTrue (r2.Active);
			Assert.IsFalse (r3.Active);
			Assert.AreSame (r2, r1.Group.ActiveRadioButton);
		}

		[Test]
		public void GroupActiveChangeEvent ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;

			int ev = 0;
			RadioButton activeRadio = null;

			r1.Group.ActiveRadioButtonChanged += delegate(object sender, EventArgs e) {
				ev++;
				activeRadio = r1.Group.ActiveRadioButton;
			};

			r1.Active = true;
			Assert.AreEqual (0, ev);

			ev = 0;
			activeRadio = null;
			r2.Active = true;
			Assert.AreEqual (1, ev);
			Assert.AreSame (r2, activeRadio);
			Assert.AreSame (r2, r1.Group.ActiveRadioButton);
			
			ev = 0;
			activeRadio = null;
			r3.Active = true;
			Assert.AreEqual (1, ev);
			Assert.AreSame (r3, activeRadio);
			Assert.AreSame (r3, r1.Group.ActiveRadioButton);
		}
		
		[Test]
		public void GroupClearActive ()
		{
			var r1 = new RadioButton ();
			var r2 = new RadioButton ();
			var r3 = new RadioButton ();
			r2.Group = r3.Group = r1.Group;
			r2.Active = true;
			Assert.IsFalse (r1.Active);
			Assert.IsTrue (r2.Active);
			Assert.IsFalse (r3.Active);

			r1.Group.ClearActive ();
			Assert.IsFalse (r1.Active);
			Assert.IsFalse (r2.Active);
			Assert.IsFalse (r3.Active);
			Assert.IsNull (r1.Group.ActiveRadioButton);
		}
	}
}

