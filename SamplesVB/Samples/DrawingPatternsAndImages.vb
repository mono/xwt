
'DrawingPatternsAndImages.vb

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
    Public Class DrawingPatternsAndImages
        Inherits Drawings

        Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
            MyBase.OnDraw(ctx, dirtyRect)
            Me.PatternsAndImages(ctx, 5.0, 5.0)
        End Sub

        Public Sub PatternsAndImages(ctx As Context, x As Double, y As Double)
            ctx.Save()
            ctx.Translate(x, y)
            ctx.SetColor(Colors.Black)

            ' Dashed lines
            ctx.SetLineDash(15.0, {10.0, 10.0, 5.0, 5.0})
            ctx.Rectangle(10.0, 10.0, 100.0, 100.0)
            ctx.Stroke()
            ctx.SetLineDash(0.0)

            ' Image
            Dim arcColor As Color = New Color(1.0, 0.0, 1.0)
            Dim ib As ImageBuilder = New ImageBuilder(30, 30, ImageFormat.ARGB32)
            ib.Context.Arc(15.0, 15.0, 15.0, 0.0, 360.0)
            ib.Context.SetColor(arcColor)
            ib.Context.Fill()
            ib.Context.SetColor(Colors.DarkKhaki)
            ib.Context.Rectangle(0.0, 0.0, 5.0, 5.0)
            ib.Context.Fill()
            Dim img As Image = ib.ToImage()
            ctx.DrawImage(img, 0.0, 0.0)
            ctx.DrawImage(img, 0.0, 50.0, 50.0, 10.0)
            ctx.Arc(100.0, 100.0, 15.0, 0.0, 360.0)
            arcColor.Alpha = 0.4
            ctx.SetColor(arcColor)
            ctx.Fill()

            ' ImagePattern

            ctx.Save()
            ctx.Translate(x + 130.0, y)
            ctx.Pattern = New ImagePattern(img)
            ctx.Rectangle(0.0, 0.0, 100.0, 100.0)
            ctx.Fill()
            ctx.Restore()
            ctx.Restore()


            ' // Setting pixels

            ctx.SetLineWidth(1.0)
            For i As Integer = 0 To 50 - 1
                For j As Integer = 0 To 50 - 1
                    Dim c As Color = Color.FromHsl(0.5, CDec(i) / 50.0, CDec(j) / 50.0)
                    ctx.Rectangle(CDec(i), CDec(j), 1.0, 1.0)
                    ctx.SetColor(c)
                    ctx.Fill()
                Next
            Next
        End Sub
    End Class
End Namespace
