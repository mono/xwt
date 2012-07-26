
'NotebookSample.vb

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
    Public Class NotebookSample
        Inherits VBox

        Public Sub New()
            Dim nb As Notebook = New Notebook()
            nb.Add(New Label("First tab content"), "First Tab")
            nb.Add(New MyWidget(), "Second Tab")
            MyBase.PackStart(nb, BoxMode.FillAndExpand)
        End Sub
    End Class

    Friend Class MyWidget
        Inherits Canvas

        Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
            ctx.SetLineWidth(5.0)
            ctx.SetColor(New Color(1.0, 0.0, 0.5))
            ctx.Rectangle(5.0, 5.0, 200.0, 100.0)
            ctx.FillPreserve()
            ctx.SetColor(New Color(0.0, 0.0, 1.0))
            ctx.Stroke()
        End Sub
    End Class

End Namespace
