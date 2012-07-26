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
            text.Text = "Lorem ipsum dolor sit amet,"
            Dim size As Size = text.GetSize()
            col.Width = size.Width
            col.Height += size.Height + 10.0
            ctx.DrawTextLayout(text, 0.0, 0.0)
            ctx.SetColor(Colors.DarkMagenta)
            text.Text = "consetetur sadipscing elitr, sed diam nonumy"
            text.Width = col.Width
            Dim size2 As Size = text.GetSize()
            ctx.DrawTextLayout(text, 0.0, col.Bottom)
            col.Height += size2.Height + 10.0
            ctx.SetColor(Colors.Black)
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
            ctx.SetLineWidth(1.0)
            ctx.SetColor(Colors.Black.WithAlpha(0.5))
            ctx.MoveTo(col.Right + 5.0, col.Top)
            ctx.LineTo(col.Right + 5.0, col.Bottom)
            ctx.Stroke()
            ctx.MoveTo(col2.Right + 5.0, col2.Top)
            ctx.LineTo(col2.Right + 5.0, col2.Bottom)
            ctx.Stroke()
            ctx.SetColor(Colors.Black)
            ctx.Save()
            text.Font = MyBase.Font.WithSize(10.0)
            text.Text = String.Format("Size 1 {0}" & vbCrLf & "Size 2 {1}" & vbCrLf & "Size 3 {2} Scale {3}", size, size2, size3, scale)
            text.Width = -1.0
            text.Height = -1.0
            ctx.Rotate(5.0)
            Dim ty As Integer = 30
            ctx.DrawTextLayout(text, CDec(ty), col.Bottom + 10.0)
            ctx.Restore()
            ctx.Restore()
            y = 180.0
            Me.DrawText(ctx, New TextLayout(Me) With {.Text = "Stright text"}, y)
            Me.DrawText(ctx, New TextLayout(Me) With {.Text = "The quick brown fox jumps over the lazy dog", .Width = 100.0}, y)
            Me.DrawText(ctx, New TextLayout(Me) With {.Text = vbLf & "Empty line above" & vbLf & "Line break above" & vbLf & vbLf & "Empty line above" & vbLf & vbLf & vbLf & "Two empty lines above" & vbLf & "Empty line below" & vbLf, .Width = 200.0}, y)
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
