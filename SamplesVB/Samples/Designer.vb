
'Designer.vb

'Author:
'      lluis <${AuthorEmail}>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2011 lluis

'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:

'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.

'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.

Imports System
Imports Xwt
Imports Xwt.Design

Namespace Samples
	Public Class Designer
		Inherits VBox

		Public Sub New()
			Dim box As VBox = New VBox()
			Dim b As Button = New Button("Hi there")
			box.PackStart(b)

			Dim la As Label = New Label("Some label")
			box.PackStart(la)

			Dim hb As HBox = New HBox()
			hb.PackStart(New Label("Text"))

			Dim cb As New ComboBox
			cb.Items.Add("One")
			cb.Items.Add("Two")
			cb.SelectedIndex = 0
			hb.PackStart(cb)
			box.PackStart(hb)

			Dim ds As DesignerSurface = New DesignerSurface()
			ds.Load(box)

			MyBase.PackStart(ds, BoxMode.FillAndExpand)
		End Sub
	End Class
End Namespace
