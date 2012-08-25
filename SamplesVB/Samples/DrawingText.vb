
'DrawingText.vb

'Author:
'      Lytico (http://limada.sourceforge.net)
'      Lluis Sanchez <lluis@xamarin.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2012 Xamarin Inc

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
	Public Class DrawingText
		Inherits Drawings

		Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
			MyBase.OnDraw(ctx, dirtyRect)
			Me.Texts(ctx, 5.0, 5.0)
		End Sub

		Public Overridable Sub Texts(ctx As Context, x As Double, y As Double)
			ctx.Save()

			ctx.Translate(x, y)

			ctx.SetColor(Colors.Black)

			Dim col As Rectangle = Nothing
			Dim col2 As Rectangle = Nothing

			Dim text As TextLayout = New TextLayout(ctx)
			text.Font = MyBase.Font.WithSize(10.0)

			' first text
			text.Text = "Lorem ipsum dolor sit amet,"
			Dim size As Size = text.GetSize()
			col.Width = size.Width
			col.Height += size.Height + 10.0
			ctx.DrawTextLayout(text, 0.0, 0.0)

			' proofing width; test should align with text above
			ctx.SetColor(Colors.DarkMagenta)
			text.Text = "consetetur sadipscing elitr, sed diam nonumy"
			text.Width = col.Width
			Dim size2 As Size = text.GetSize()

			ctx.DrawTextLayout(text, 0.0, col.Bottom)
			col.Height += size2.Height + 10.0

			ctx.SetColor(Colors.Black)

			' proofing scale, on second col
			ctx.Save()
			ctx.SetColor(Colors.Red)
			col2.Left = col.Right + 10.0

			text.Text = "eirmod tempor invidunt ut."

			Dim scale As Double = 1.2
			text.Width /= scale
			Dim size3 As Size = text.GetSize()
			col2.Height = size3.Height * scale
			col2.Width = size3.Width * scale + 5.0
			ctx.Scale(scale, scale)
			ctx.DrawTextLayout(text, col2.Left / scale, col2.Top / scale)
			ctx.Restore()

			' proofing heigth, on second col
			ctx.Save()
			ctx.SetColor(Colors.DarkCyan)
			text.Text = "Praesent ac lacus nec dolor pulvinar feugiat a id elit."
			text.Height = text.GetSize().Height / 2.0
			text.Trimming = TextTrimming.WordElipsis
			ctx.DrawTextLayout(text, col2.Left, col2.Bottom + 5.0)
			ctx.SetLineWidth(1.0)
			ctx.SetColor(Colors.Blue)
			ctx.Rectangle(New Rectangle(col2.Left, col2.Bottom + 5.0, text.Width, text.Height))
			ctx.Stroke()
			ctx.Restore()

			' drawing col line
			ctx.SetLineWidth(1.0)

			ctx.SetColor(Colors.Black.WithAlpha(0.5))
			ctx.MoveTo(col.Right + 5.0, col.Top)
			ctx.LineTo(col.Right + 5.0, col.Bottom)
			ctx.Stroke()
			ctx.MoveTo(col2.Right + 5.0, col2.Top)
			ctx.LineTo(col2.Right + 5.0, col2.Bottom)
			ctx.Stroke()
			ctx.SetColor(Colors.Black)

			' proofing rotate, and printing size to see the values
			ctx.Save()

			text.Font = MyBase.Font.WithSize(10.0)
			text.Text = String.Format("Size 1 {0}" & vbCrLf & "Size 2 {1}" & vbCrLf & "Size 3 {2} Scale {3}", size, size2, size3, scale)

			' this clears textsize
			text.Width = -1.0
			text.Height = -1.0
			ctx.Rotate(5.0)

			' maybe someone knows a formula with angle and textsize to calculyte ty
			Dim ty As Integer = 30
			ctx.DrawTextLayout(text, CDec(ty), col.Bottom + 10.0)

			ctx.Restore()

			' scale example here:
			ctx.Restore()

			' Text boces
			y = 180.0

			' Without wrapping
			Me.DrawText(ctx, New TextLayout(Me) With {.Text = "Stright text"}, y)

			' With wrapping
			Me.DrawText(ctx, New TextLayout(Me) With {.Text = "The quick brown fox jumps over the lazy dog", _
														.Width = 100.0}, y)

			' With blank lines
			Me.DrawText(ctx, New TextLayout(Me) With {.Text = vbLf & "Empty line above" & vbLf _
														& "Line break above" & vbLf & vbLf _
														& "Empty line above" & vbLf & vbLf & vbLf _
														& "Two empty lines above" & vbLf _
														& "Empty line below" & vbLf, .Width = 200.0}, y)
		End Sub

		Private Sub DrawText(ctx As Context, tl As TextLayout, ByRef y As Double)
			Dim x As Double = 10.0
			Dim s As Size = tl.GetSize()
			Dim rect As Rectangle = New Rectangle(x, y, s.Width, s.Height).Inflate(0.5, 0.5)
			ctx.SetLineWidth(1.0)
			ctx.SetColor(Colors.Blue)
			ctx.Rectangle(rect)
			ctx.Stroke()
			ctx.SetColor(Colors.Black)
			ctx.DrawTextLayout(tl, x, y)

			y += s.Height + 20.0
		End Sub
	End Class
End Namespace
