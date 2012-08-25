
'CanvasWithWidget.vb

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
	Public Class CanvasWithWidget
		Inherits VBox

		Public Sub New()
			Dim c As MyCanvas = New MyCanvas()
			MyBase.PackStart(c, BoxMode.FillAndExpand)
		End Sub
	End Class

	Friend Class MyCanvas
		Inherits Canvas

		Private rect As Rectangle = New Rectangle(30.0, 30.0, 100.0, 30.0)

		Public Sub New()
			Dim entry As TextEntry = New TextEntry() With {.ShowFrame = False}
			MyBase.AddChild(entry, Me.rect)
			Dim box As HBox = New HBox()
			box.PackStart(New Button("..."))
			box.PackStart(New TextEntry(), BoxMode.FillAndExpand)
			MyBase.AddChild(box, New Rectangle(30.0, 70.0, 100.0, 30.0))
		End Sub

		Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
			ctx.Rectangle(0.0, 0.0, MyBase.Bounds.Width, MyBase.Bounds.Height)
			Dim g As LinearGradient = New LinearGradient(0.0, 0.0, MyBase.Bounds.Width, MyBase.Bounds.Height)
			g.AddColorStop(0.0, New Color(1.0, 0.0, 0.0))
			g.AddColorStop(1.0, New Color(0.0, 1.0, 0.0))
			ctx.Pattern = g
			ctx.Fill()

			Dim r As Rectangle = Me.rect.Inflate(5.0, 5.0)
			ctx.Rectangle(r)
			ctx.SetColor(New Color(0.0, 0.0, 1.0))
			ctx.SetLineWidth(1.0)
			ctx.Stroke()
		End Sub
	End Class

End Namespace
