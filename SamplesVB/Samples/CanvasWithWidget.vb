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
