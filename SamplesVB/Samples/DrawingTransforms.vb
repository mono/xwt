
'DrawingTransforms.vb

'Authors:
'      Lluis Sanchez <lluis@xamarin.com>
'      Lytico (http://limada.sourceforge.net)
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
    Public Class Drawings
        Inherits Canvas

        Public Sub New()
            MyBase.BackgroundColor = Colors.White
        End Sub
    End Class

    Public Class DrawingTransforms
        Inherits Canvas

        Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
            MyBase.OnDraw(ctx, dirtyRect)
            Me.Transforms(ctx, 5.0, 5.0)
        End Sub

        Public Overridable Sub Transforms(ctx As Context, x As Double, y As Double)
            Me.Rotate(ctx, x, y)
            Me.Scale(ctx, x + 120.0, y)
        End Sub

        Public Overridable Sub Rotate(ctx As Context, x As Double, y As Double)
            ctx.Save()
            ctx.Translate(x + 30.0, y + 30.0)
            ctx.SetLineWidth(3.0)

            ' Rotation
            Dim [end] As Double = 270.0
            Dim r As Double = 30.0

            Dim i As Double = 0.0
            While i <= [end]
                ctx.Save()
                ctx.Rotate(i)
                ctx.MoveTo(0.0, 0.0)
                ctx.RelLineTo(r, 0.0)
                Dim c As Double = i / [end]
                ctx.SetColor(New Color(c, c, c))
                ctx.Stroke()

                ' Visual test for TransformPoints
                Dim p0 As Point = New Point(0.0, 0.0)
                Dim p As Point = New Point(0.0, -r)
                Dim p2(2) As Point
                p2(0) = p0
                p2(1) = p

                ctx.TransformPoints(p2)
                ctx.ResetTransform()
                ctx.Translate(2.0 * r + 1.0, 0.0)
                ctx.MoveTo(p2(0))
                ctx.LineTo(p2(1))
                c = 1.0 - c
                ctx.SetColor(New Color(c, c, c))
                ctx.Stroke()
                ctx.Restore()
                i += 5.0
            End While

            ctx.Restore()
        End Sub

        Public Overridable Sub Scale(ctx As Context, ax As Double, ay As Double)
            ctx.Save()
            ctx.Translate(ax, ay)
            ctx.SetColor(Colors.Black)
            ctx.SetLineWidth(1.0)

            Dim x As Double = 0.0
            Dim y As Double = 0.0
            Dim w As Double = 10.0
            Dim inc As Double = 0.1
            Dim i As Double = inc

            While i < 3.5
                ctx.Save()
                ctx.Scale(i, i)
                ctx.Rectangle(x, y, w, w)
                ctx.SetColor(Colors.Yellow.WithAlpha(1.0 / i))
                ctx.FillPreserve()
                ctx.SetColor(Colors.Red.WithAlpha(1.0 / i))
                ctx.Stroke()
                x = x + w * inc
                y = y + w * inc / 3.0
                ctx.MoveTo(x, y)
                ctx.Restore()
                i += inc
            End While

            ctx.Restore()
        End Sub
    End Class

End Namespace
