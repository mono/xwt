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
