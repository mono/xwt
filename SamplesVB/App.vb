Imports System
Imports Xwt

Namespace Samples
    Public Class App
        Public Shared Sub Run(engineType As String)
            Application.Initialize(engineType)
            Dim w As MainWindow = New MainWindow()
            w.Title = "Xwt VB.NET Demo Application"
            w.Width = 500.0
            w.Height = 400.0
            w.Show()
            Application.Run()
            w.Dispose()
        End Sub
    End Class
End Namespace
