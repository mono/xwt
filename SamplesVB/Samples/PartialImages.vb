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
            Me.img = Image.FromResource(MyBase.[GetType](), "cow.jpg")
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
