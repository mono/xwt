
'ScrollWindowSample.vb

'Author:
'      Lluis Sanchez <lluis@xamarin.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2011 Xamarin Inc

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
	Public Class ScrollWindowSample
		Inherits VBox

		Public Sub New()
			Dim v As ScrollView = New ScrollView()
			Dim b1 As VBox = New VBox()
			For i As Integer = 0 To 30 - 1
				b1.PackStart(New Label("Line " & i), BoxMode.None)
			Next

			Dim u As Button = New Button("Click to remove")
			AddHandler u.Clicked, Sub()
									  b1.Remove(u)
								  End Sub
			b1.PackStart(u)

			v.Content = b1
			v.VerticalScrollPolicy = ScrollPolicy.Always
			v.BorderVisible = False
			MyBase.PackStart(v, BoxMode.FillAndExpand)

			Dim v2 As ScrollView = New ScrollView()
			Dim b2 As VBox = New VBox()
			For i As Integer = 0 To 10 - 1
				b2.PackStart(New Label("Line " & i), BoxMode.None)
			Next
			v2.Content = b2
			v2.VerticalScrollPolicy = ScrollPolicy.Never
			MyBase.PackStart(v2, BoxMode.FillAndExpand)

			Dim v3 As ScrollView = New ScrollView()
			Dim b3 As VBox = New VBox()
			Dim b4 As Button = New Button("Click to add items")
			AddHandler b4.Clicked, Sub()
									   For j As Integer = 0 To 10 - 1
										   b3.PackStart(New Label("Line " & j), BoxMode.None)
									   Next
								   End Sub
			b3.PackStart(b4)
			v3.Content = b3
			v3.VerticalScrollPolicy = ScrollPolicy.Automatic
			MyBase.PackStart(v3, BoxMode.FillAndExpand)

			Dim v4 As ScrollView = New ScrollView()
			MyBase.PackStart(v4, BoxMode.FillAndExpand)
			Dim sb As ScrollableCanvas = New ScrollableCanvas()
			v4.Content = sb
			v4.VerticalScrollPolicy = ScrollPolicy.Always
		End Sub
	End Class

	Friend Class ScrollableCanvas
		Inherits Canvas

		Private hscroll As ScrollAdjustment
		Private vscroll As ScrollAdjustment
		Private imageSize As Integer = 500

		Protected Overrides ReadOnly Property SupportsCustomScrolling() As Boolean
			Get
				Return True
			End Get
		End Property

		Public Sub New()
			MyBase.MinWidth = 100.0
			MyBase.MinHeight = 100.0
		End Sub

		Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
			ctx.Translate(-Me.hscroll.Value, -Me.vscroll.Value)
			ctx.Rectangle(New Rectangle(0.0, 0.0, CDec(Me.imageSize), CDec(Me.imageSize)))
			ctx.SetColor(Colors.White)
			ctx.Fill()
			ctx.Arc(CDec((Me.imageSize / 2)), CDec((Me.imageSize / 2)), CDec((Me.imageSize / 2 - 20)), 0.0, 360.0)
			ctx.SetColor(New Color(0.0, 0.0, 1.0))
			ctx.Fill()
			ctx.ResetTransform()

			ctx.Rectangle(0.0, 0.0, MyBase.Bounds.Width, 30.0)
			ctx.SetColor(New Color(1.0, 0.0, 0.0, 0.5))
			ctx.Fill()
		End Sub

		Protected Overrides Sub SetScrollAdjustments(horizontal As ScrollAdjustment, vertical As ScrollAdjustment)
			Me.hscroll = horizontal
			Me.vscroll = vertical

			Me.hscroll.UpperValue = CDec(Me.imageSize)
			Me.hscroll.PageIncrement = MyBase.Bounds.Width
			Me.hscroll.PageSize = MyBase.Bounds.Width
			AddHandler Me.hscroll.ValueChanged, Sub()
													MyBase.QueueDraw()
												End Sub

			Me.vscroll.UpperValue = CDec(Me.imageSize)
			Me.vscroll.PageIncrement = MyBase.Bounds.Height
			Me.vscroll.PageSize = MyBase.Bounds.Height
			AddHandler Me.vscroll.ValueChanged, Sub()
													MyBase.QueueDraw()
												End Sub
		End Sub

		Protected Overrides Sub OnBoundsChanged()
			Me.vscroll.PageIncrement = MyBase.Bounds.Height
			Me.vscroll.PageSize = Me.vscroll.PageIncrement

			Me.hscroll.PageIncrement = MyBase.Bounds.Width
			Me.hscroll.PageSize = Me.hscroll.PageIncrement
		End Sub
	End Class

End Namespace
