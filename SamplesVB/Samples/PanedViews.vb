Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class PanedViews
        Inherits HPaned

        Public Sub New()
            MyBase.Panel1.Content = Me.CreateFrame("Fixed panel at the left", False)
            Dim centralPaned As HPaned = New HPaned()
            centralPaned.Panel1.Content = Me.CreateFrame("Should expand" & vbLf & "horizontally and vertically", True)
            centralPaned.Panel1.Resize = True
            Dim verticalPaned As VPaned = New VPaned()
            verticalPaned.Panel1.Content = Me.CreateFrame("Fixed panel" & vbLf & "at the top", False)
            verticalPaned.Panel2.Content = Me.CreateFrame("Should expand vertically", True)
            verticalPaned.Panel2.Resize = True
            centralPaned.Panel2.Content = verticalPaned
            MyBase.Panel2.Content = centralPaned
            MyBase.Panel2.Resize = True
        End Sub

        Private Function CreateFrame(text As String, fixedp As Boolean) As Frame
            Dim f As Frame = New Frame(FrameType.Custom)
            f.BorderColor = (If(fixedp, New Color(1.0, 0.0, 0.0), New Color(0.0, 0.0, 1.0)))
            f.BorderWidth.SetAll(1.0)
            f.Margin.SetAll(10.0)
            f.Content = New Label(text)
            f.Content.Margin.SetAll(10.0)
            Return f
        End Function
    End Class
End Namespace
