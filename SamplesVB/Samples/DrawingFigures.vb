Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class DrawingFigures
        Inherits Drawings

        Protected Overrides Sub OnDraw(ctx As Context, dirtyRect As Rectangle)
            MyBase.OnDraw(ctx, dirtyRect)
            Me.Figures(ctx, 5.0, 25.0)
        End Sub

        Public Overridable Sub Figures(ctx As Context, x As Double, y As Double)
            Me.Lines(ctx)
            Me.Rectangles(ctx, x, y + 20.0)
            Me.Curves1(ctx, x, y + 80.0)
            Me.Curves2(ctx, x + 100.0, y + 80.0)
        End Sub

        Public Sub Lines(ctx As Context)
            ctx.Save()
            ctx.SetColor(Colors.Black)
            Dim nPairs As Integer = 4
            Dim length As Double = 90.0
            Dim gap As Double = 2.0
            ctx.SetLineWidth(1.0)
            Dim x As Double = 0.0
            Dim y As Double = 0.5
            Dim [end] As Double = x + 2.0 * (length - 1.0) + gap
            ctx.MoveTo(x, y)
            ctx.LineTo([end], y)
            ctx.Stroke()
            y = 4.5
            For w As Integer = 1 To nPairs
                x = 0.0
                ctx.SetLineWidth(CDec(w))
                ctx.MoveTo(x, y)
                ctx.RelLineTo(length - 1.0, 0.0)
                ctx.Stroke()
                ctx.SetLineWidth(CDec((w + 1)))
                x += gap + length - 1.0
                ctx.MoveTo(x, y)
                ctx.RelLineTo(length - 1.0, 0.0)
                ctx.Stroke()
                y += CDec((w * 2)) + gap
            Next
            ctx.Restore()
        End Sub

        Public Overridable Sub Rectangles(ctx As Context, x As Double, y As Double)
            ctx.Save()
            ctx.Translate(x, y)
            ctx.SetLineWidth(1.0)
            ctx.Rectangle(0.0, 0.0, 10.0, 10.0)
            ctx.SetColor(Colors.Black)
            ctx.Fill()
            ctx.Rectangle(15.0, 0.0, 10.0, 10.0)
            ctx.SetColor(Colors.Black)
            ctx.Stroke()
            ctx.SetLineWidth(3.0)
            ctx.Rectangle(0.0, 15.0, 10.0, 10.0)
            ctx.SetColor(Colors.Black)
            ctx.Fill()
            ctx.Rectangle(15.0, 15.0, 10.0, 10.0)
            ctx.SetColor(Colors.Black)
            ctx.Stroke()
            ctx.Restore()
            ctx.Save()
            ctx.Translate(x + 50.0, y)
            ctx.Rectangle(0.0, 0.0, 40.0, 40.0)
            ctx.MoveTo(35.0, 35.0)
            ctx.RelLineTo(0.0, -20.0)
            ctx.RelLineTo(-20.0, 0.0)
            ctx.RelLineTo(0.0, 20.0)
            ctx.ClosePath()
            ctx.SetColor(Colors.Black)
            ctx.Fill()
            ctx.Restore()
            ctx.Save()
            ctx.Translate(x + 120.0, y)
            Dim r As Integer = 5
            Dim i As Integer = 0
            Dim t As Integer = 0
            Dim w As Integer = 50
            Dim h As Integer = 30
            ctx.SetColor(Colors.Black)
            ctx.Arc(CDec((i + r)), CDec((t + r)), CDec(r), 180.0, 270.0)
            ctx.Arc(CDec((i + w - r)), CDec((t + r)), CDec(r), 270.0, 0.0)
            ctx.Arc(CDec((i + w - r)), CDec((t + h - r)), CDec(r), 0.0, 90.0)
            ctx.Arc(CDec((i + r)), CDec((t + h - r)), CDec(r), 90.0, 180.0)
            ctx.ClosePath()
            ctx.StrokePreserve()
            ctx.SetColor(Colors.AntiqueWhite)
            ctx.Fill()
            ctx.Restore()
        End Sub

        Public Overridable Sub Curves1(ctx As Context, x As Double, y As Double)
            ctx.Save()
            ctx.Translate(x, y)
            ctx.SetLineWidth(1.0)
            Dim curve1 As Action = Sub()
                                       ctx.MoveTo(0.0, 30.0)
                                       ctx.CurveTo(20.0, 0.0, 50.0, 0.0, 60.0, 25.0)
                                   End Sub
            Dim curve2 As Action = Sub()
                                       ctx.LineTo(0.0, 0.0)
                                       ctx.CurveTo(20.0, 30.0, 50.0, 30.0, 60.0, 5.0)
                                   End Sub
            Dim paint As Action = Sub()
                                      curve1()
                                      curve2()
                                      ctx.ClosePath()
                                      ctx.SetColor(New Color(0.0, 0.0, 0.0, 0.5))
                                      ctx.StrokePreserve()
                                      ctx.SetColor(New Color(1.0, 0.0, 1.0, 0.5))
                                      ctx.Fill()
                                  End Sub
            paint()
            ctx.Translate(0.0, 40.0)
            curve2 = Sub()
                         ctx.MoveTo(0.0, 0.0)
                         ctx.CurveTo(20.0, 30.0, 50.0, 30.0, 60.0, 5.0)
                     End Sub
            paint()
            ctx.Restore()
        End Sub

        Public Overridable Sub Curves2(ctx As Context, sx As Double, sy As Double)
            ctx.Save()
            ctx.Translate(sx, sy)
            ctx.SetColor(Colors.Black)
            Dim x As Double = 0.0
            Dim y As Double = 40.0
            Dim x2 As Double = y - x
            Dim y2 As Double = x2 + y
            Dim x3 As Double = x + y
            Dim y3 As Double = x
            Dim x4 As Double = y2
            Dim y4 As Double = y
            ctx.MoveTo(x, y)
            ctx.CurveTo(x2, y2, x3, y3, x4, y4)
            ctx.SetLineWidth(2.0)
            ctx.Stroke()
            ctx.SetColor(New Color(1.0, 0.2, 0.2, 0.6))
            ctx.SetLineWidth(1.0)
            ctx.MoveTo(x, y)
            ctx.LineTo(x2, y2)
            ctx.MoveTo(x3, y3)
            ctx.LineTo(x4, y4)
            ctx.Stroke()
            ctx.Restore()
        End Sub
    End Class
End Namespace
