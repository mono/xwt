
'Boxes.vb

'Author:
'      Peter Gill <peter@majorsilence.com>

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
Imports Xwt.Drawing

Namespace Samples
	Public Class Boxes
		Inherits VBox

		Public Sub New()

			Dim box As HBox = New HBox()
			Dim box2 As VBox = New VBox()
			box2.PackStart(New SimpleBox(30.0), BoxMode.None)
			box2.PackStart(New SimpleBox(30.0), BoxMode.None)
			box2.PackStart(New SimpleBox(30.0), BoxMode.FillAndExpand)
			box.PackStart(box2, BoxMode.FillAndExpand)
			box.PackStart(New SimpleBox(30.0), BoxMode.None)
			box.PackStart(New SimpleBox(30.0), BoxMode.Expand)
			MyBase.PackStart(box, BoxMode.None)

			Dim box3 As HBox = New HBox()
			box3.PackEnd(New SimpleBox(30.0))
			box3.PackStart(New SimpleBox(20.0) With {.Color = New Color(1.0, 0.5, 0.5)})
			box3.PackEnd(New SimpleBox(40.0))
			box3.PackStart(New SimpleBox(10.0) With {.Color = New Color(1.0, 0.5, 0.5)})
			box3.PackEnd(New SimpleBox(30.0))
			box3.PackStart(New SimpleBox(10.0) With {.Color = New Color(1.0, 0.5, 0.5)}, BoxMode.FillAndExpand)
			MyBase.PackStart(box3)

			Dim box4 As HBox = New HBox()
			Dim b As Button = New Button("Click me")
			AddHandler b.Clicked, Sub()
									  b.Label = "Button has grown"
								  End Sub
			box4.PackStart(New SimpleBox(30.0), BoxMode.FillAndExpand)
			box4.PackStart(b)
			box4.PackStart(New SimpleBox(30.0), BoxMode.FillAndExpand)
			MyBase.PackStart(box4)

			Dim box5 As HBox = New HBox()
			Dim b2 As Button = New Button("Hide / Show")
			box5.PackStart(New SimpleBox(30.0), BoxMode.FillAndExpand)

			Dim hsb As SimpleBox = New SimpleBox(20.0)
			box5.PackStart(hsb, BoxMode.None)
			box5.PackStart(b2)
			box5.PackStart(New SimpleBox(30.0), BoxMode.FillAndExpand)
			AddHandler b2.Clicked, Sub()
									   hsb.Visible = Not hsb.Visible
								   End Sub

			MyBase.PackStart(box5)


			Dim box6 As HBox = New HBox()
			For i As Integer = 0 To 15 - 1
				box6.PackStart(New Label("TestLabel" & i) With {.MinWidth = 10.0})
			Next
			MyBase.PackStart(box6)

		End Sub
	End Class

	Friend Class SimpleBox
		Inherits Canvas

		Private coreSize As Size

		Private margin As Double = 1.0

		Private highlight As Boolean

		Public Property Color() As Color

		Public Sub New(coreSize As Double)
			Me.Color = New Color(0.5, 0.5, 1.0)
			Me.coreSize = New Size(coreSize, coreSize)
			MyBase.MinHeight = coreSize + Me.margin * 2.0
			MyBase.MinWidth = MyBase.MinHeight
		End Sub

		Public Sub New(coreWidth As Double, coreHeight As Double)
			Me.Color = New Color(0.5, 0.5, 1.0)
			Me.coreSize = New Size(coreWidth, coreHeight)
			MyBase.MinWidth = Me.coreSize.Width + Me.margin * 2.0
			MyBase.MinHeight = Me.coreSize.Height + Me.margin * 2.0
		End Sub

		Protected Overrides Sub OnMouseEntered(args As EventArgs)
			MyBase.OnMouseEntered(args)
			Me.highlight = True
			MyBase.QueueDraw()
		End Sub

		Protected Overrides Sub OnMouseExited(args As EventArgs)
			MyBase.OnMouseExited(args)
			MyBase.QueueDraw()
			Me.highlight = False
		End Sub

		Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
			ctx.SetColor(New Color(0.5, 0.5, 0.5))
			ctx.Rectangle(MyBase.Bounds)
			ctx.Fill()
			ctx.SetColor(New Color(0.8, 0.8, 0.8))
			ctx.Rectangle(MyBase.Bounds.Inflate(-Me.margin, -Me.margin))
			ctx.Fill()
			ctx.SetColor(If(Me.highlight, Me.Color.BlendWith(Colors.White, 0.5), Me.Color))
			ctx.Rectangle(MyBase.Bounds.Width / 2.0 - Me.coreSize.Width / 2.0, MyBase.Bounds.Height / 2.0 - Me.coreSize.Height / 2.0, Me.coreSize.Width, Me.coreSize.Height)
			ctx.Fill()
		End Sub
	End Class
End Namespace
