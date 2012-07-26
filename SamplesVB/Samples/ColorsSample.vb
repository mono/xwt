Imports System
Imports Xwt

Namespace Samples
    Public Class ColorsSample
        Inherits VBox

        Public Sub New()
            Dim la As Label = New Label("Normal color")
            la.BackgroundColor = la.BackgroundColor
            MyBase.PackStart(la)
        End Sub
    End Class
End Namespace
