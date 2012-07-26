Imports System
Imports System.Timers
Imports Xwt

Namespace Samples
    Public Class ProgressBarSample
        Inherits VBox

        Private timer As Timer = New Timer(100.0)

        Private determinateProgressBar As ProgressBar

        Private indeterminateProgressBar As ProgressBar

        Public Sub New()
            Me.indeterminateProgressBar = New ProgressBar()
            MyBase.PackStart(Me.indeterminateProgressBar, BoxMode.FillAndExpand)
            Me.indeterminateProgressBar.Indeterminate = True
            AddHandler Me.timer.Elapsed, AddressOf Me.Increase
            Me.determinateProgressBar = New ProgressBar()
            Me.determinateProgressBar.Fraction = 0.0
            MyBase.PackStart(Me.determinateProgressBar, BoxMode.FillAndExpand)
            Me.timer.Start()
        End Sub

        Public Sub Increase(sender As Object, args As ElapsedEventArgs)
            Dim currentFraction As Double? = New Double?(Me.determinateProgressBar.Fraction)
            Dim nextFraction As Double
            If currentFraction.HasValue AndAlso currentFraction.Value >= 0.0 AndAlso currentFraction.Value <= 0.9 Then
                nextFraction = currentFraction.Value + 0.1
            Else
                nextFraction = 0.0
            End If
            Application.Invoke(Sub()
                                   Me.determinateProgressBar.Fraction = nextFraction
                               End Sub)
        End Sub
    End Class
End Namespace
