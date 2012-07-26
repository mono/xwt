Imports System
Imports Xwt

Namespace Samples
    Public Class WidgetEvents
        Inherits VBox

        Public Sub New()
            Dim la As Label = New Label("Move the mouse here")
            MyBase.PackStart(la)
            Dim res As Label = New Label()
            MyBase.PackStart(res)
            AddHandler la.MouseEntered, Sub()
                                            res.Text = "Mouse is inside the label"
                                        End Sub

            AddHandler la.MouseExited, Sub()
                                           res.Text = ""
                                       End Sub
        End Sub
    End Class
End Namespace
