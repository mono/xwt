
'PartialImages.vb

'Author:
'      Luís Reis <luiscubal@gmail.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2012 Luís Reis

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
    Public Class PartialImages
        Inherits VBox

        Public Sub New()
            Dim canvas As PartialImageCanvas = New PartialImageCanvas()
            MyBase.PackStart(canvas, BoxMode.FillAndExpand)
        End Sub
    End Class

    Friend Class PartialImageCanvas
        Inherits Canvas

        Private img As Image

        Public Sub New()

            Dim ms As New IO.MemoryStream()
            SamplesVB.My.Resources.cow.Save(ms, System.Drawing.Imaging.ImageFormat.Png)

            Me.img = Image.FromStream(ms)
        End Sub

        Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
            MyBase.OnDraw(ctx, dirtyRect)
            Dim y As Integer = 0
            While CDec(y) < Me.img.Size.Height / 50.0
                Dim x As Integer = 0
                While CDec(x) < Me.img.Size.Width / 50.0
                    ctx.DrawImage(Me.img, New Rectangle(CDec((x * 50)), CDec((y * 50)), 50.0, 50.0), New Rectangle(CDec((x * 55)), CDec((y * 55)), 50.0, 50.0))
                    x += 1
                End While
                y += 1
            End While
        End Sub
    End Class

End Namespace
