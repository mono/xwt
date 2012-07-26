Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class Frames
        Inherits VBox

        Public Sub New()
            Dim f As New Frame()
            f.Label = "Simple widget box"
            f.Content = New SimpleBox(50)
            PackStart(f)

            f = New Frame()
            f.Content = New Label("No label")
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.Left = 1
            f.BorderWidth.Top = 2
            f.BorderWidth.Right = 3
            f.BorderWidth.Bottom = 4
            f.BorderColor = New Color(0, 0, 1)
            f.Content = New Label("Custom")
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.SetAll(2)
            f.Padding.Left = 10
            f.Padding.Top = 20
            f.Padding.Right = 30
            f.Padding.Bottom = 40
            f.Content = New SimpleBox(50)
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.SetAll(2)
            f.Padding.SetAll(10)
            f.Content = New Label("With red background")
            f.BackgroundColor = New Color(1, 0, 0)
            PackStart(f)
        End Sub
    End Class
End Namespace
