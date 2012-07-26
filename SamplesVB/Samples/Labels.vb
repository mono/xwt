Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class Labels
        Inherits VBox

        Public Sub New()
            Dim la As Label = New Label("Simple label")
            PackStart(la)

            PackStart(New Label("Label with red background") With {.BackgroundColor = New Color(1.0, 0.0, 0.0)})
        End Sub
    End Class
End Namespace
