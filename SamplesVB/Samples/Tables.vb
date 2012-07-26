Imports System
Imports Xwt

Namespace Samples
    Public Class Tables
        Inherits VBox

        Public Sub New()
            Dim t As Table = New Table()
            Dim b As SimpleBox = New SimpleBox(200.0, 20.0)
            t.Attach(b, 0, 1, 0, 1)
            b = New SimpleBox(5.0, 20.0)
            t.Attach(b, 1, 2, 0, 1)
            b = New SimpleBox(250.0, 20.0)
            t.Attach(b, 0, 2, 1, 2, AttachOptions.Expand, AttachOptions.Expand)
            b = New SimpleBox(300.0, 20.0)
            t.Attach(b, 1, 3, 2, 3)
            b = New SimpleBox(100.0, 20.0)
            t.Attach(b, 2, 3, 3, 4)
            b = New SimpleBox(450.0, 20.0)
            t.Attach(b, 0, 3, 4, 5)
            MyBase.PackStart(t)
            Dim box As HBox = New HBox()
            MyBase.PackStart(box)
            t = New Table()
            t.Attach(New Label("One:"), 0, 1, 0, 1)
            t.Attach(New TextEntry(), 1, 2, 0, 1)
            t.Attach(New Label("Two:"), 0, 1, 1, 2)
            t.Attach(New TextEntry(), 1, 2, 1, 2)
            t.Attach(New Label("Three:"), 0, 1, 2, 3)
            t.Attach(New TextEntry(), 1, 2, 2, 3)
            box.PackStart(t)
        End Sub
    End Class
End Namespace
