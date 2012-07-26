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
            ctx.SetLineDash(15.0, {10.0, 10.0, 5.0, 5.0})
            ctx.Rectangle(10.0, 10.0, 100.0, 100.0)
            ctx.Stroke()
            ctx.SetLineDash(0.0, 0D)
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
            ctx.Save()
            ctx.Translate(x + 130.0, y)
            ctx.Pattern = New ImagePattern(img)
            ctx.Rectangle(0.0, 0.0, 100.0, 100.0)
            ctx.Fill()
            ctx.Restore()
            ctx.Restore()
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
