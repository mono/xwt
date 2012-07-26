Imports System
Imports Xwt

Namespace Samples
    Public Class Checkboxes
        Inherits VBox

        Public Sub New()
            MyBase.PackStart(New CheckBox("Normal checkbox"))
            MyBase.PackStart(New CheckBox("Mixed to start") With {.Mixed = True})
            Dim b As CheckBox = New CheckBox("Allows mixed") With {.AllowMixed = True}
            MyBase.PackStart(b)
            Dim clicks As Integer = 0
            Dim toggles As Integer = 0
            Dim la As Label = New Label()
            MyBase.PackStart(la)
            AddHandler b.Clicked, Sub()
                                      clicks += 1
                                      la.Text = String.Format("active:{0}, mixed:{1}, clicks:{2}, toggles:{3}", b.Active, b.Mixed, clicks, toggles)
                                  End Sub
            AddHandler b.Toggled, Sub()
                                      toggles += 1
                                      la.Text = String.Format("active:{0}, mixed:{1}, clicks:{2}, toggles:{3}", b.Active, b.Mixed, clicks, toggles)
                                  End Sub
        End Sub
    End Class
End Namespace
